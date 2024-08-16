// Copyright QUANTOWER LLC. © 2017-2023. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.InteropServices;
using TradingPlatform.BusinessLayer;
using static System.Net.Mime.MediaTypeNames;
using TradingPlatform.BusinessLayer.Integration;
using TradingPlatform.BusinessLayer.Utils;
using TradingPlatform.BusinessLayer.Chart;

namespace IndicatorChandelierExit
{
	public class IndicatorChandelierExit : Indicator
    {
        [InputParameter("ATR Period", 0, 1, 9999)]
        public int Period = 22;
        [InputParameter("ATR Multiplier", 0, 1, 9999)]
        public double multipl = 3;

        public override string ShortName => $"CE ({this.Period}; {this.multipl})";

        private double longStop, longStopPrev, shortStop, shortStopPrev;
        private int dir;

        private Indicator atr;
        public IndicatorChandelierExit()
            : base()
        {
            Name = "Chandelier Exit";
            Description = "The Chandelier Exit is a popular tool among traders used to help determine appropriate stop loss levels.";

            AddLineSeries("Sell", Color.Red, 2, LineStyle.Solid);
            AddLineSeries("Buy", Color.Green, 2, LineStyle.Solid);

            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            this.atr = Core.Indicators.BuiltIn.ATR(this.Period, MaMode.SMA, Indicator.DEFAULT_CALCULATION_TYPE);
            this.AddIndicator(this.atr);
            dir = 1;
            longStop = High() - atr.GetValue() * multipl;
            shortStop = Low() + atr.GetValue() * multipl;
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            longStopPrev = longStop;
            longStop = High() - atr.GetValue() * multipl;
            if (GetPrice(PriceType.Close) > longStopPrev)
	        longStop = Math.Max(longStop, longStopPrev);

            shortStopPrev = shortStop;
            shortStop = Low() + atr.GetValue() * multipl;
            if (GetPrice(PriceType.Close) < shortStopPrev)
	        shortStop = Math.Min(shortStop, shortStopPrev);

            if (GetPrice(PriceType.Close) > shortStopPrev)
                dir = 1;
            else if (GetPrice(PriceType.Close) < longStopPrev)
                dir = -1;

            if (dir == 1) SetValue(longStop, 1);
            else SetValue(shortStop, 0);
        }
    }
}
