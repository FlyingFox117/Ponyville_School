using PonyvilleSchool2._0.Models;
using System.Windows;
using System.Windows.Controls;

namespace PonyvilleSchool2._0.Services
{
    //Инструмент для преобразования объектов
    public class ContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MatchContent content)
            {
                return content.type switch
                {
                    "image" => ImageTemplate,
                    _ => TextTemplate
                };
            }
            return base.SelectTemplate(item, container);
        }
    }
    public class DragCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DragCard card)
            {
                return card.Content.type switch
                {
                    "image" => ImageTemplate,
                    _ => TextTemplate
                };
            }
            return base.SelectTemplate(item, container);
        }
    }

    public class TestContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TestContent content)
            {
                return content.type switch
                {
                    "image" => ImageTemplate,
                    _ => TextTemplate
                };
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class TestAnswerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TestAnswer answer)
            {
                return answer.content?.type switch
                {
                    "image" => ImageTemplate,
                    _ => TextTemplate
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
