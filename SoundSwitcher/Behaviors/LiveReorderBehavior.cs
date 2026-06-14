using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SoundSwitcher.Behaviors
{
    public static class LiveReorderBehavior
    {
        public static readonly DependencyProperty IsReorderGripProperty =
            DependencyProperty.RegisterAttached("IsReorderGrip", typeof(bool), typeof(LiveReorderBehavior), new PropertyMetadata(false, OnIsReorderGripChanged));

        public static bool GetIsReorderGrip(DependencyObject obj) => (bool)obj.GetValue(IsReorderGripProperty);

        public static void SetIsReorderGrip(DependencyObject obj, bool value) => obj.SetValue(IsReorderGripProperty, value);

        private static FrameworkElement? _draggedItem;
        private static ItemsControl? _itemsControl;
        private static Point _startMousePos;
        private static int _originalIndex;
        private static int _currentIndex;
        private static List<FrameworkElement>? _containers;
        private static bool _isFinishing;

        private static void OnIsReorderGripChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                    element.MouseMove += Element_MouseMove;
                    element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
                    element.LostMouseCapture += Element_LostMouseCapture;
                }
                else
                {
                    element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                    element.MouseMove -= Element_MouseMove;
                    element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                    element.LostMouseCapture -= Element_LostMouseCapture;
                }
            }
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement grip)
            {
                _draggedItem = FindAncestor<ContentPresenter>(grip) as FrameworkElement
                               ?? FindAncestor<ListBoxItem>(grip);

                _itemsControl = FindAncestor<ItemsControl>(grip);

                if (_draggedItem != null && _itemsControl != null)
                {
                    _originalIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedItem);
                    if (_originalIndex < 0) return;

                    _currentIndex = _originalIndex;
                    _startMousePos = e.GetPosition(_itemsControl);

                    _containers = [];

                    for (int i = 0; i < _itemsControl.Items.Count; i++)
                    {
                        if (_itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
                            _containers.Add(container);
                    }

                    Panel.SetZIndex(_draggedItem, 1000);

                    if (VisualTreeHelper.GetChildrenCount(_draggedItem) > 0 && VisualTreeHelper.GetChild(_draggedItem, 0) is FrameworkElement child)
                    {
                        var scale = GetOrCreateTransform<ScaleTransform>(child);
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.02, TimeSpan.FromSeconds(0.2)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.02, TimeSpan.FromSeconds(0.2)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                    }

                    if (_itemsControl.DataContext is ViewModels.MainViewModel mainViewModel)
                    {
                        mainViewModel.IsReordering = true;
                    }

                    grip.CaptureMouse();
                    e.Handled = true;
                }
            }
        }

        private static void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedItem != null && _itemsControl != null && _containers != null && Mouse.Captured?.Equals(sender as UIElement) == true)
            {
                var currentMousePos = e.GetPosition(_itemsControl);
                double deltaY = currentMousePos.Y - _startMousePos.Y;

                SetTranslateY(_draggedItem, deltaY, 0);

                int steps = (int)Math.Round(deltaY / _draggedItem.ActualHeight);
                int newIndex = _originalIndex + steps;
                newIndex = Math.Max(0, Math.Min(newIndex, _containers.Count - 1));

                if (newIndex != _currentIndex)
                {
                    _currentIndex = newIndex;
                    UpdateOthers();
                }
            }
        }

        private static void UpdateOthers()
        {
            if (_containers == null || _draggedItem == null) return;

            for (int i = 0; i < _containers.Count; i++)
            {
                if (i == _originalIndex) continue;

                double targetY = 0;

                if (_currentIndex < _originalIndex)
                {
                    if (i >= _currentIndex && i < _originalIndex)
                        targetY = _draggedItem.ActualHeight;
                }
                else if (_currentIndex > _originalIndex)
                {
                    if (i > _originalIndex && i <= _currentIndex)
                        targetY = -_draggedItem.ActualHeight;
                }

                SetTranslateY(_containers[i], targetY, 0.35);
            }
        }

        private static TTransform GetOrCreateTransform<TTransform>(FrameworkElement child) where TTransform : Transform, new()
        {
            if (child.RenderTransform is { IsFrozen: true })
            {
                child.RenderTransform = child.RenderTransform.Clone();
            }

            switch (child.RenderTransform)
            {
                case TTransform single:
                    return single;

                case TransformGroup group:
                {
                    foreach (var t in group.Children)
                    {
                        if (t is TTransform found) return found;
                    }

                    var newTransform = new TTransform();
                    group.Children.Add(newTransform);
                    return newTransform;
                }
            }

            var newGroup = new TransformGroup();

            if (child.RenderTransform != null && child.RenderTransform != Transform.Identity)
            {
                newGroup.Children.Add(child.RenderTransform);
            }

            var targetTransform = new TTransform();
            newGroup.Children.Add(targetTransform);
            child.RenderTransform = newGroup;

            return targetTransform;
        }

        private static void SetTranslateY(FrameworkElement element, double y, double durationSeconds)
        {
            if (VisualTreeHelper.GetChildrenCount(element) > 0 && VisualTreeHelper.GetChild(element, 0) is FrameworkElement child)
            {
                var transform = GetOrCreateTransform<TranslateTransform>(child);

                if (durationSeconds > 0)
                {
                    var anim = new DoubleAnimation
                    {
                        To = y,
                        Duration = TimeSpan.FromSeconds(durationSeconds),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };

                    transform.BeginAnimation(TranslateTransform.YProperty, anim);
                }
                else
                {
                    transform.BeginAnimation(TranslateTransform.YProperty, null);
                    transform.Y = y;
                }
            }
        }

        private static void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FinishDrag(sender as UIElement);
        }

        private static void Element_LostMouseCapture(object sender, MouseEventArgs e)
        {
            FinishDrag(sender as UIElement);
        }

        private static void FinishDrag(UIElement? grip)
        {
            if (_draggedItem == null || _isFinishing) return;

            _isFinishing = true;

            try
            {
                grip?.ReleaseMouseCapture();

                var item = _draggedItem;
                List<FrameworkElement>? containers = _containers;
                var itemsControl = _itemsControl;

                Panel.SetZIndex(item, 0);

                if (VisualTreeHelper.GetChildrenCount(item) > 0 && VisualTreeHelper.GetChild(item, 0) is FrameworkElement child)
                {
                    var scale = GetOrCreateTransform<ScaleTransform>(child);
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.2)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.2)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                }

                if (containers != null)
                {
                    foreach (var container in containers)
                    {
                        SetTranslateY(container, 0, 0);
                    }
                }

                if (_currentIndex != _originalIndex && itemsControl != null)
                {
                    var source = itemsControl.ItemsSource;
                    var moveMethod = source?.GetType().GetMethod("Move");

                    if (moveMethod != null)
                    {
                        moveMethod.Invoke(source, [_originalIndex, _currentIndex]);
                    }
                    else if (source is IList list)
                    {
                        var listItem = list[_originalIndex];
                        list.RemoveAt(_originalIndex);
                        list.Insert(_currentIndex, listItem);
                    }
                }
            }
            finally
            {
                if (_itemsControl?.DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    mainViewModel.IsReordering = false;
                }

                _draggedItem = null;
                _itemsControl = null;
                _containers = null;
                _isFinishing = false;
            }
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
