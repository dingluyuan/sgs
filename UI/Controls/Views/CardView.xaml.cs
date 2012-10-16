﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Diagnostics;

using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class SuitColorToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SuitColorType color = (SuitColorType)value;
            if (color == SuitColorType.Black)
            {
                return new SolidColorBrush(Colors.Black);
            }
            else if (color == SuitColorType.Red)
            {
                Color red = new Color();
                red.R = 212;
                red.A = 255;
                return new SolidColorBrush(red);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for CardView.xaml
    /// </summary>
    public partial class CardView : UserControl
    {
        public CardView()
        {
            InitializeComponent();
            _daMoveX = new DoubleAnimation();
            _daMoveY = new DoubleAnimation();
            _daOpacity = new DoubleAnimation();
            CardMoveDurationSeconds = 0.5;
            ChangeOpacityDurationSeconds = 0.5;
            Storyboard.SetTarget(_daMoveX, this);
            Storyboard.SetTarget(_daMoveY, this);
            Storyboard.SetTarget(_daOpacity, this);
            _DisappearAfterMoveHandler = new EventHandler(_DisappearAfterMove);
        }

        public static double WidthHeightRatio = 0.7154;

        private static void OnFadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView cardView = d as CardView;
            if (cardView == null) return;
            Storyboard fadeAnimation;
            if ((bool)e.NewValue == true)
            {
                fadeAnimation = cardView.Resources["sbFade"] as Storyboard;                
            }
            else
            {
                fadeAnimation = cardView.Resources["sbUnfade"] as Storyboard;
            }
            Trace.Assert(fadeAnimation != null);
            fadeAnimation.Begin();
        }

        DoubleAnimation _daMoveX;
        DoubleAnimation _daMoveY;
        DoubleAnimation _daOpacity;

        public double CardMoveDurationSeconds { get; set; }

        public double ChangeOpacityDurationSeconds { get; set; }

        public void Rebase()
        {            
            if (Position == null) return;
            if (Parent == null || !(Parent is Canvas)) return;
            double x = (double)GetValue(Canvas.LeftProperty);
            double y = (double)GetValue(Canvas.TopProperty);
            if (double.IsNaN(x) || double.IsNaN(y))
            {
                SetValue(Canvas.LeftProperty, Position.X);
                SetValue(Canvas.TopProperty, Position.Y);
                return;
            }
            Point point = new Point(x, y);
            double destX = Position.X + Offset.X;
            double destY = Position.Y + Offset.Y;
            if ((x == destX) && (y == destY))
            {
                return;
            }
            _daMoveX.To = destX;
            _daMoveY.To = destY;            
            _daMoveX.Duration = TimeSpan.FromSeconds(CardMoveDurationSeconds);
            _daMoveY.Duration = TimeSpan.FromSeconds(CardMoveDurationSeconds);
            BeginAnimation(Canvas.LeftProperty, null);
            BeginAnimation(Canvas.TopProperty, null);
            if (DisappearAfterMove)
            {
                _daMoveX.Completed += _DisappearAfterMoveHandler;
            }
            BeginAnimation(Canvas.LeftProperty, _daMoveX);
            BeginAnimation(Canvas.TopProperty, _daMoveY);
        }

        private EventHandler _DisappearAfterMoveHandler;

        private void _DisappearAfterMove(object sender, EventArgs e)
        {
            _daMoveX.Completed -= _DisappearAfterMoveHandler;            
            CardOpacity = 0;
        }

        private static void OnCardOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView card = d as CardView;
            if (card == null) return;
            if (card.Opacity == card.CardOpacity)
            {
                return;
            }
            card._daOpacity.To = card.CardOpacity;
            card._daOpacity.Duration = TimeSpan.FromSeconds(card.ChangeOpacityDurationSeconds);
            card.BeginAnimation(CardView.OpacityProperty, card._daOpacity);
        }

        private static void OnOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView card = d as CardView;
            if (card == null) return;
            card.Rebase();
        }       

        #region Dependency Properties

        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0, 0),
                new PropertyChangedCallback(OnOffsetPropertyChanged)));
        
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0,0)));

        public double CardOpacity
        {
            get { return (double)GetValue(CardOpacityProperty); }
            set { SetValue(CardOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardOpacityProperty =
            DependencyProperty.Register("CardOpacity", typeof(double), typeof(CardView), new UIPropertyMetadata(0d,
                                        new PropertyChangedCallback(OnCardOpacityChanged)));

        public bool DisappearAfterMove
        {
            get;
            set;
        }

        public bool IsFaded
        {
            get { return (bool)GetValue(IsFadedProperty); }
            set { SetValue(IsFadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFadedProperty =
            DependencyProperty.Register("IsFaded", typeof(bool), typeof(CardView), new UIPropertyMetadata(false, 
                new PropertyChangedCallback(OnFadedChanged)));       

        #endregion

        #region Card Creation Helpers
        public static CardView CreateCard(Card card, int width = 93, int height = 130)
        {
            return new CardView()
            {
                DataContext = new CardViewModel() { Card = card },
                Width = width,
                Height = height
            };
        }

        public static IList<CardView> CreateCards(IList<Card> cards)
        {
            List<CardView> cardViews = new List<CardView>();
            foreach (Card card in cards)
            {
                cardViews.Add(CreateCard(card));
            }
            return cardViews;
        }
        #endregion
    }
}
