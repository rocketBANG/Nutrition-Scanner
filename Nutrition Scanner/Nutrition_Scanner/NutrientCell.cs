using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nutrition_Scanner
{
    public class NutrientCell : ViewCell
    {
        public NutrientCell()
        {
            //instantiate each of our views
            var name = new Entry();
            var value = new Entry();

            StackLayout cellWrapper = new StackLayout();
            StackLayout horizontalLayout = new StackLayout();
            //Label left = new Label();
            //Label right = new Label();

            //set bindings
            name.SetBinding(Entry.TextProperty, "Name");
            value.SetBinding(Entry.TextProperty, "Value");

            //Set properties for desired design
            cellWrapper.BackgroundColor = Color.FromHex("#eee");
            horizontalLayout.Orientation = StackOrientation.Horizontal;
            value.HorizontalOptions = LayoutOptions.EndAndExpand;

            //add views to the view hierarchy
            horizontalLayout.Children.Add(name);
            horizontalLayout.Children.Add(value);
            cellWrapper.Children.Add(horizontalLayout);
            View = cellWrapper;

        }
    }
}
