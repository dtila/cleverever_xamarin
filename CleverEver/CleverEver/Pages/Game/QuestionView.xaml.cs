using CleverEver.Composition;
using CleverEver.Controls;
using CleverEver.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XButton = Xamarin.Forms.Button;

namespace CleverEver.Pages.Game
{
    public partial class QuestionView : ContentView
    {
        private const string HightlightOptionStyle = "HightlightOptionStyle";
        private const string CorrectOptionHightlightStyle = "CorrectOptionHightlightStyle";
        private const string WrongOptionHightlightStyle = "WrongOptionHightlightStyle";


        private Style _normalQuestionStyle, _selectedQuestionStyle;
        private XButton _selectedAnswerButton;
        private bool _canAnswer;
        private ITextMeter _textMeter;

        public QuestionViewModel ViewModel
        {
            get { return BindingContext as QuestionViewModel; }
        }

        public QuestionView()
        {
            InitializeComponent();

            _canAnswer = true;
            _selectedQuestionStyle = (Style)Resources["HightlightOptionStyle"];
            _normalQuestionStyle = (Style)Resources["NormalOptionStyle"];
            _textMeter = CleverEver.Composition.DependencyContainer.Resolve<ITextMeter>();

            this.LayoutChanged += QuestionView_LayoutChanged;
            this.BindingContextChanged += QuestionView_BindingContextChanged;
        }

        private Style FinalAnswerNormalStyle
        {
            get { return (Style)Resources["NormalAnswerStyle"]; }
        }
        private Style FinalAnswerHightlightStyle
        {
            get { return (Style)Resources["HightlightAnswerStyle"]; }
        }

        private void QuestionView_BindingContextChanged(object sender, EventArgs e)
        {
            var vm = BindingContext as QuestionViewModel;
            if (vm != null)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private async void QuestionView_LayoutChanged(object sender, EventArgs e)
        {
            await PlayAnimations();
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Question))
            {
                MeasureText();

                ResetAnimations();
                foreach (View button in grid.Children)
                    button.Style = _normalQuestionStyle;
                finalAnswer.Style = FinalAnswerNormalStyle;

                await PlayAnimations();
                _canAnswer = true;
            }
        }

        private void MeasureText()
        {
            var max = GetLongestAnswer();
            grid.ColumnCount = max.Length > 17 ? 1 : 2;

            /*var columns = 2;
            var width = Math.Max(grid.Width + grid.Margin.HorizontalThickness + grid.Padding.HorizontalThickness, 0);
            var fontSize = Device.GetNamedSize(NamedSize.Small, finalAnswer);

            while (columns > 0)
            {
                var size = _textMeter.MeasureTextSize(max, grid.Width, fontSize);
                var desiredWidth = size.Width + 20;
                if (desiredWidth < grid.Width / columns)
                    columns++;
                break;
            }

            var maxColumns = ViewModel.Answers.Length;*/
            
        }

        private string GetLongestAnswer()
        {
            if (ViewModel.Answers == null || ViewModel.Answers.Length == 0)
                throw new InvalidOperationException("The answers are empty or null");

            string max = ViewModel.Answers[0];
            for (int i = 1; i < ViewModel.Answers.Length; i++)
                if (ViewModel.Answers[i].Length > max.Length)
                    max = ViewModel.Answers[i];
            return max;
        }

        private void Question_Clicked(object sender, EventArgs e)
        {
            var button = sender as XButton;
            foreach (var abutton in grid.Children)
            {
                abutton.Style = abutton != button ? _normalQuestionStyle : _selectedQuestionStyle;
            }

            finalAnswer.Style = FinalAnswerHightlightStyle;
            _selectedAnswerButton = button;
        }

        private async void Answer_Clicked(object sender, EventArgs e)
        {
            if (_selectedAnswerButton == null || !_canAnswer)
                return;

            _canAnswer = false;
            var selectedAnswerIndex = QuestionsResponsiveGrid.GetIndex(_selectedAnswerButton);

            var hightingButton = _selectedAnswerButton;
            var hightingStyle = (Style)Resources[WrongOptionHightlightStyle];

            int correctIndex;
            if (ViewModel.IsAnswerCorrect(selectedAnswerIndex, out correctIndex))
            {
                hightingStyle = (Style)Resources[CorrectOptionHightlightStyle];
            }
            else
            {
                grid.Children[correctIndex].Style = (Style)Resources[CorrectOptionHightlightStyle];
            }

            var selected = true;
            for (int i=0; i<5; i++)
            {
                hightingButton.Style = selected ? hightingStyle : _normalQuestionStyle;

                selected = !selected;
                await Task.Delay(200);
            }

            ViewModel.Answer(selectedAnswerIndex);
            _selectedAnswerButton = null;
        }

        private async Task PlayAnimations()
        {
            //await finalAnswer.ScaleTo(1.1, 200, Easing.SpringOut);
        }

        private void ResetAnimations()
        {
            finalAnswer.Scale = 1;
        }
    }
}
