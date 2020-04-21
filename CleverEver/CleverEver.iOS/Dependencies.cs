using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DryIoc;
using Xamarin.Forms;
using CleverEver.Droid;
using CleverEver.Platform;
using CleverEver.Composition;

namespace CleverEver.Droid.Infrastructure
{
    class Dependencies
    {
        public static void Setup()
        {
            var container = new DryIoc.Container(Rules.Default.WithTrackingDisposableTransients());

            container.Register<Rendering.ITextMeter, CleverEver.iOS.Rendering.TextMeterImplementation>();

            DependencyContainer.Container = container;
        }
    }
}