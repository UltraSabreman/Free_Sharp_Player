using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Free_Sharp_Player{


	[TemplatePart(Name = "txt_textBlock1", Type = typeof(TextBlock))]
	[TemplatePart(Name = "txt_textBlock2", Type = typeof(TextBlock))]
	[TemplatePart(Name = "cnv_container", Type = typeof(Canvas))]
	public class MarqueeTextBlock : Label {
		private Canvas theCanvas;
		private TextBlock textBlock1;
		private TextBlock textBlock2;


		private DoubleAnimation text1Anim;
		private DoubleAnimation text2Anim;

		public double TextPosVertical {
			get { return (double)GetValue(TextPosVerticalProperty); }
			private set { SetValue(TextPosVerticalProperty, value); }
		}
		public static readonly DependencyProperty TextPosVerticalProperty = DependencyProperty.Register("TextPosVertical", typeof(double), typeof(MarqueeTextBlock), null);

		public double TextPosHorizontal {
			get { return (double)GetValue(TextPosHorizontalProperty); }
			private set { SetValue(TextPosHorizontalProperty, value); }
		}
		public static readonly DependencyProperty TextPosHorizontalProperty = DependencyProperty.Register("TextPosHorizontal", typeof(double), typeof(MarqueeTextBlock), null);


		static MarqueeTextBlock() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MarqueeTextBlock), new FrameworkPropertyMetadata(typeof(MarqueeTextBlock)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			theCanvas = GetTemplateChild("cnv_container") as Canvas;
			textBlock1 = GetTemplateChild("txt_textBlock1") as TextBlock;
			textBlock2 = GetTemplateChild("txt_textBlock2") as TextBlock;
		}

		protected override void OnContentChanged(object oldContent, object newContent) {
			base.OnContentChanged(oldContent, newContent);

			if (oldContent as String != newContent as String)
				DoMarqueeLogic();
		}


		//TODO: handle stretch vertical allingment and all horizontal ones.
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			base.OnRenderSizeChanged(sizeInfo);

			switch (VerticalContentAlignment) {
				case System.Windows.VerticalAlignment.Center:
					TextPosVertical = (theCanvas.ActualHeight / 2) - (textBlock1.ActualHeight / 2);
					break;
				case System.Windows.VerticalAlignment.Bottom:
					TextPosVertical = theCanvas.ActualHeight - (textBlock1.ActualHeight / 2);
					break;
				default:
					TextPosVertical = 0;
					break;
			}

			/*switch (HorizontalContentAlignment) {
				case System.Windows.HorizontalAlignment.Center:
					TextPosHorizontal = (theCanvas.ActualWidth / 2) - (textBlock1.ActualWidth / 2);
					break;
				case System.Windows.HorizontalAlignment.Right:
					TextPosHorizontal = textBlock1.ActualWidth / 2;
					break;
				default:
					TextPosHorizontal = 0;
					break;
			}*/

			

			DoMarqueeLogic();
		}


		private void DoMarqueeLogic() {
			if (theCanvas == null) return;

			if (theCanvas != null && textBlock1 != null && textBlock1.ActualWidth >= theCanvas.ActualWidth) {
				textBlock2.Visibility = Visibility.Visible;

				/*if (HorizontalContentAlignment == System.Windows.HorizontalAlignment.Right) {
					text1Anim = new DoubleAnimation((textBlock1.ActualWidth + theCanvas.ActualWidth / 2), 0, new Duration(new TimeSpan(0, 0, 10)));
					text2Anim = new DoubleAnimation(0, -(textBlock1.ActualWidth + theCanvas.ActualWidth / 2), new Duration(new TimeSpan(0, 0, 10)));
				} else {*/
					text1Anim = new DoubleAnimation(0, (textBlock1.ActualWidth + theCanvas.ActualWidth / 2), new Duration(new TimeSpan(0, 0, 10)));
					text2Anim = new DoubleAnimation(-(textBlock1.ActualWidth + theCanvas.ActualWidth / 2), 0, new Duration(new TimeSpan(0, 0, 10)));
				//}

				text1Anim.RepeatBehavior = RepeatBehavior.Forever;
				text2Anim.RepeatBehavior = RepeatBehavior.Forever;

				textBlock1.BeginAnimation(Canvas.RightProperty, text1Anim);
				textBlock2.BeginAnimation(Canvas.RightProperty, text2Anim);
			} else {
				textBlock2.Visibility = Visibility.Hidden;
				textBlock1.BeginAnimation(Canvas.RightProperty, null);
				textBlock2.BeginAnimation(Canvas.RightProperty, null);
			}

		}
	}
}
