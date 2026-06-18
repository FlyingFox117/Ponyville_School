using PonyvilleSchool2._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PonyvilleSchool2._0.Services
{
    //Инструмент для преобразования объектов - карточек на MatchBlock
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
}
