using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverEver.Controls
{
    public class QuestionsResponsiveGrid : Grid
    {
        public static readonly BindableProperty ColumnsCountProperty = BindableProperty.Create(nameof(ColumnCount), typeof(int), typeof(QuestionsResponsiveGrid), 2,
            propertyChanged: QuestionsResponsiveGrid_BindingPropertyChangedDelegate);

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(QuestionsResponsiveGrid));

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(QuestionsResponsiveGrid),
            propertyChanged: QuestionsResponsiveGrid_OnItemsSourceChanged);

        public static readonly BindableProperty IndexProperty = BindableProperty.CreateAttached("Index", typeof(int), typeof(QuestionsResponsiveGrid), 0);

        public static int GetIndex(BindableObject view)
        {
            return (int)view.GetValue(IndexProperty);
        }

        public static void SetIndex(BindableObject view, int value)
        {
            view.SetValue(IndexProperty, value);
        }

        private bool _playAnimations;

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public int ColumnCount
        {
            get { return (int)GetValue(ColumnsCountProperty); }
            set { SetValue(ColumnsCountProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static void QuestionsResponsiveGrid_BindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var grid = bindable as QuestionsResponsiveGrid;
            grid.RearangeItems();
        }

        public static void QuestionsResponsiveGrid_OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var grid = bindable as QuestionsResponsiveGrid;
            grid.OnItemsSourceChanged();
        }

        protected override void OnAdded(View view)
        {
            base.OnAdded(view);

            RarangeItem(view, Children.Count - 1);
        }

        protected override async void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            if (!_playAnimations)
                return;

            _playAnimations = false;
            var tasks = new Task[Children.Count];
            for (var i = 0; i < Children.Count; i++)
            {
                var negative = i % 2 == 0;
                var _x = (negative ? -1 : 1) * (width + width / 2);
                tasks[i] = Children[i].TranslateTo(_x, 0, easing: Easing.CubicOut);
            }

            await Task.WhenAll(tasks);

            /*for (int i = 0; i < Children.Count; i++)
            {
                var view = Children[i];
                var negative = i % 2 == 0;
                view.TranslationX = (negative ? -1 : 1) * width;
            }*/

            await PlayAnimations().ConfigureAwait(false);
        }

        private void OnItemsSourceChanged()
        {
            RowDefinitions.Clear();
            RowDefinitions.Clear();

            if (ItemsSource != null)
            {
                var i = 0;
                foreach (var data in ItemsSource)
                {
                    var count = Children.Count;
                    if (i < count)
                    {
                        Children[i].BindingContext = data;
                        RarangeItem(Children[i], i);
                    }
                    else
                    {
                        var view = ItemTemplate.CreateContent() as View;
                        if (view == null)
                            throw new InvalidOperationException("The ItemTemplate content is not a view");

                        view.BindingContext = data;
                        Children.Add(view);
                    }

                    i++;
                }

                // Remove extra items
                for (int j = Children.Count - 1; j >= i; j--)
                    Children.RemoveAt(j);

                _playAnimations = true;
                InvalidateLayout();
            }
        }

        private void RearangeItems()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                RarangeItem(Children[i], i);
            }
        }

        private void RarangeItem(View view, int index)
        {
            var column = index % ColumnCount;
            var row = index / ColumnCount;

            if (RowDefinitions.Count < row)
                RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });

            if (ColumnDefinitions.Count < column)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

            SetIndex(view, index);
            SetColumn(view, column);
            SetRow(view, row);
        }

        private void ResetAnimation(View view, int index)
        {
            var negative = index % 2 == 0;
            view.TranslationX = (negative ? -1 : 1) * WidthRequest;
        }

        public void ResetAnimations()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                ResetAnimation(Children[i], i);
            }
        }

        private async Task PlayAnimations()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                await Children[i].TranslateTo(0, 0, easing: Easing.CubicInOut);
            }
        }
    }
}
