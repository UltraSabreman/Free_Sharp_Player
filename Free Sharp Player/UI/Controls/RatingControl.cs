using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Plugin_Base;

namespace Free_Sharp_Player {
	public class RatingControl : Control {
		//private static object theLock = new object();
		//private static Dictionary<String, Tuple<String, String, int>> ratingMap = new Dictionary<string, Tuple<string, string, int>>();

		static RatingControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
		}

		private String Votes {
			get { return (String)GetValue(VotesProperty); }
			set { SetValue(VotesProperty, value); }
		}

		public Track BoundTrack {
			get { return (Track)GetValue(BoundTrackProperty); }
			set { 
				SetValue(BoundTrackProperty, value);
				boundTrack = value;
				Dispatcher.Invoke(new Action(() => {
					if (BoundTrack == null)
						IsEnabled = false;
					else
						IsEnabled = true;
					ChangeVoteMode();
				}));
			}
		}

		public bool ShowRating {
			get { return (bool)GetValue(ShowRatingProperty); }
			set { 
				SetValue(ShowRatingProperty, value);
				Dispatcher.Invoke(new Action(() => {
					OnRenderSizeChanged(null);
					RatingVisibilityChange();
				}));
			}
		}

		public double ButtonWidth {
			get { return (double)GetValue(ButtonWidthProperty); }
			set { SetValue(ButtonWidthProperty, value); }
		}


		public static readonly DependencyProperty VotesProperty = DependencyProperty.Register("Votes", typeof(String), typeof(RatingControl), null);
		public static readonly DependencyProperty ShowRatingProperty = DependencyProperty.Register("ShowRating", typeof(bool), typeof(RatingControl), null);
		public static readonly DependencyProperty BoundTrackProperty = DependencyProperty.Register("BoundTrack", typeof(Track), typeof(RatingControl), new PropertyMetadata(BoundTrackChange));
		public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register("ButtonWidth", typeof(double), typeof(RatingControl), new PropertyMetadata(RatingVisibilityChange));

		private static void BoundTrackChange(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			RatingControl h = sender as RatingControl;

			if (h != null) {
				h.boundTrack = h.BoundTrack;
				if (h.BoundTrack == null)
					h.IsEnabled = false;
				else
					h.IsEnabled = true;
				h.ChangeVoteMode();
			}
		}
		private static void RatingVisibilityChange(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			RatingControl h = sender as RatingControl;
			if (h != null) h.RatingVisibilityChange();
		}

		private Button LikeButton;
		private Button DislikeButton;
		private Label Rating;
		private ColumnDefinition RatingColumn1;
		private ColumnDefinition RatingColumn2;

		private int myVote = 0;
		private int voteCount = 0;
		private Track boundTrack = null;

		public RatingControl() {
			BoundTrack = null;

		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			LikeButton = GetTemplateChild("btn_Like") as Button;
			DislikeButton = GetTemplateChild("btn_Dislike") as Button;
			Rating = GetTemplateChild("lbl_Votes") as Label;
			RatingColumn1 = GetTemplateChild("RatingColumn1") as ColumnDefinition;
			RatingColumn2 = GetTemplateChild("RatingColumn2") as ColumnDefinition;


			//RatingVisibilityChange();

			LikeButton.Click += Liked;
			DislikeButton.Click += Disliked;
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			if (ShowRating)
				ButtonWidth = ActualWidth / 4;
			else
				ButtonWidth = ActualWidth / 2;
		}

		private void ChangeVoteMode() {
			if (IsEnabled && BoundTrack != null) {
				Votes = BoundTrack.Rating.ToString();
				voteCount = int.Parse(Votes);
			} else {
				Votes = "---";
				voteCount = 0;
				IsEnabled = false;
			}
		}

		private void RatingVisibilityChange() {
			if (Rating == null) return;
			if (!ShowRating) {
				Rating.Visibility = System.Windows.Visibility.Collapsed;
				RatingColumn1.Width = new GridLength(0);
				RatingColumn2.Width = new GridLength(0);
			} else {
				Rating.Visibility = System.Windows.Visibility.Visible;
				RatingColumn1.Width = GridLength.Auto;
				RatingColumn2.Width = GridLength.Auto;
			}
		}


		public void Liked(Object o, EventArgs e) {
			if (myVote == 0) {
				myVote = 1;
				voteCount += 1;
			} else if (myVote == 1) {
				myVote = 0;
				voteCount -= 1;
			} else if (myVote == -1) {
				myVote = 0;
				voteCount += 1;
			}

			Votes = voteCount.ToString();

            //TODO: Integrate Plugin
			//setVote.doPost(myVote, BoundTrack.trackID);
			Update();
		}


		public void Disliked(Object o, EventArgs e) {
			if (myVote == 0) {
				myVote = -1;
				voteCount -= 1;
			} else if (myVote == 1) {
				myVote = 0;
				voteCount -= 1;
			} else if (myVote == -1) {
				myVote = 0;
				voteCount += 1;
			}

			Votes = voteCount.ToString();
            //TODO: Integrate Plugin
            //setVote.doPost(myVote, BoundTrack.trackID);
            Update();
		}

		public void Update() {
			new Thread(() => {
                //TODO: Integrate Plugin
                Track tempTrack = null;// getTrack.GetSingleTrack(boundTrack.trackID);
				if (tempTrack != null) {
					String rating = tempTrack.Rating.ToString();
					voteCount = int.Parse(rating);

                    //TODO: Integrate Plugin
                    var status = -1;// getVoteStatus.doPost(boundTrack.trackID);
                    myVote = -1;// (status.vote != null ? (int)status.vote : 0);

					Dispatcher.Invoke(new Action(() => {
						Votes = rating;

						if (myVote == 1) {
							(LikeButton.Content as Rectangle).Fill = Brushes.LightGray;
							LikeButton.Background = goodColor;

							DislikeButton.Background = Brushes.LightGray;
							(DislikeButton.Content as Rectangle).Fill = badColor;
						} else if (myVote == -1) {
							(LikeButton.Content as Rectangle).Fill = goodColor;
							LikeButton.Background = Brushes.LightGray;

							DislikeButton.Background = badColor;
							(DislikeButton.Content as Rectangle).Fill = Brushes.LightGray;
						} else {
							(LikeButton.Content as Rectangle).Fill = goodColor;
							LikeButton.Background = Brushes.LightGray;

							DislikeButton.Background = Brushes.LightGray;
							(DislikeButton.Content as Rectangle).Fill = badColor;
						}
					}));
				}
			}).Start();
		}


		private Brush badColor = new BrushConverter().ConvertFrom("#770000") as Brush;
		private Brush goodColor = new BrushConverter().ConvertFrom("#007700") as Brush;

		/*private void UpdateRating() {
			lock (theLock) {
				bool ratingSet = false;
				String newRating = null;

				if (boundTrack == null || String.IsNullOrEmpty(boundTrack.trackID) || boundTrack.trackID == "0") {
				} else {
					if (!ratingMap.ContainsKey(boundTrack.trackID)) {
						int rating = int.Parse(boundTrack.rating);
						if (myVote != 0) //TODO reset somewhere
							newRating = (rating += myVote).ToString();

						ratingMap.Add(boundTrack.trackID, new Tuple<String, String, int>(rating.ToString(), newRating, myVote));
					} else {
						var ratingInfo = ratingMap[boundTrack.trackID];

						if (boundTrack.rating == ratingInfo.Item1) {
							boundTrack.rating = ratingInfo.Item2;
							newRating = ratingInfo.Item2;
							myVote = ratingInfo.Item3;
						} else {
							ratingInfo.Item1 = boundTrack.rating;
							ratingInfo.Item2 = ratingInfo.Item2;

						}

					}
				}
				if (!ratingSet) {
					Votes = "---";
					voteCount = 0;
				} else {
					Votes = newRating;
					voteCount = int.Parse(newRating);
				}
			}
		}*/

	}


}
