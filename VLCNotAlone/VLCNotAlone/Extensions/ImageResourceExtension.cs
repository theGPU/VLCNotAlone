using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using System.Xml;

namespace VLCNotAlone.Extensions
{
    [ContentProperty("Source")]
    class ImageResourceExtension : IMarkupExtension
    {
        public string Source { set; get; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            return imageSource;
        }
    }
}
