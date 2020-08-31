using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace NEU.IPGateway.UI.Views
{
    class GridLengthStarAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthStarAnimation();
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthStarAnimation));


        public GridLength From
        {
            get
            {
                return (GridLength)GetValue(FromProperty);
            }
            set
            {
                SetValue(FromProperty, value);
            }
        }


        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthStarAnimation));

        public GridLength To
        {
            get
            {
                return (GridLength)GetValue(ToProperty);
            }
            set
            {
                SetValue(ToProperty, value);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = From.Value;

            if (From.GridUnitType == GridUnitType.Auto)
                fromVal = ((GridLength)defaultOriginValue).Value;

            double toVal = ((GridLength)GetValue(ToProperty)).Value;

            return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Star);
        }
    }
}
