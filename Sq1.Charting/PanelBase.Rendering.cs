﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using Sq1.Core;
using Sq1.Core.DataTypes;
using Sq1.Core.Indicators;

namespace Sq1.Charting {
	public partial class PanelBase {
		// virtual will allow indicator panes to have their own backgrounds different to the price&volume backgrounds
		protected virtual void GutterRightBottomDrawBackground(Graphics g) {
			if (this.GutterRightDraw) {
				int catchingZero = this.ChartControl.CalculateGutterWidthNecessaryToFitPriceVolumeLabels(g);
				if (catchingZero == 0) {
					return;
				}
				this.ChartControl.GutterRightWidth_cached = catchingZero; 
				Rectangle gutterRightRect = default(Rectangle);
				gutterRightRect.X = this.PanelWidthMinusRightPriceGutter;
				gutterRightRect.Width = this.ChartControl.GutterRightWidth_cached;
				gutterRightRect.Y = 0;
				gutterRightRect.Height = this.PanelHeightMinusGutterBottomHeight;
				g.FillRectangle(this.ChartControl.ChartSettings.BrushGutterRightBackground, gutterRightRect);
			}
			if (this.GutterBottomDraw) {
				Rectangle gutterBottomRect = default(Rectangle);
				gutterBottomRect.X = 0;
				gutterBottomRect.Width = base.Width;
				gutterBottomRect.Y = this.PanelHeightMinusGutterBottomHeight;
				gutterBottomRect.Height = this.GutterBottomHeight_cached;
				g.FillRectangle(this.ChartControl.ChartSettings.BrushGutterBottomBackground, gutterBottomRect);
			}
		}
		// virtual will allow indicator panes to have their own backgrounds different to the price&volume backgrounds
		protected virtual void GutterGridLinesRightBottomDrawForeground(Graphics g) {
			string msig = " GutterGridLinesRightBottomDrawForeground() " + this.BarsIdent + " " + this.Parent.ToString();

			//MOVED_TO_OnPaintBackgroundDoubleBuffered_TO_GET_PROPER_HEIGHT_EARLIER this.ensureFontMetricsAreCalculated(g);
			
			if (this.ChartControl.BarsEmpty) return;
			if (this.VisibleMax_cached == 0) return;	//it's a cached version for once-per-render calculation
			if (double.IsNegativeInfinity(this.VisibleRangeWithTwoSqueezers_cached)) {
				Debugger.Break();
				return;
			}
			if (this.PanelHeightMinusGutterBottomHeight_cached <= 0) {
				string msg = "[" + this.PanelName + "]-PANEL_HEIGHT_MUST_BE_POSITIVE_this.PanelHeightMinusGutterBottomHeight_cached["
					+ this.PanelHeightMinusGutterBottomHeight_cached + "]";
				msg += "; this.ChartSettings.ScrollPositionAtBarIndex seems to be ZERO (Chartontrol.cs:82) => move the slider to change&serialize";
				Assembler.PopupException(msg + msig, null, false);
				return;
			}
			if (this.GutterRightFontHeight_cached <= 0) {
				string msg = "[" + this.PanelName + "]-GUTTER_FONT_HEIGHT_MUST_BE_POSITIVE_this.GutterRightFontHeight_cached["
					+ this.GutterRightFontHeight_cached + "]]";
				Assembler.PopupException(msg + msig, null, false);
				return;
			}
			
			double panelValueForBarMouseOvered = this.PanelValueForBarMouseOveredNaNunsafe;
			bool mouseTrack = this.ChartControl.ChartSettings.MousePositionTrackOnGutters;
			if (mouseTrack) {		// && this.moveHorizontalYprev > -1
				int mouseY = this.moveHorizontalYprev;
				if (this.mouseOver == false || this.moveHorizontalYprev == -1) {
					if (double.IsNaN(panelValueForBarMouseOvered) == false) {
						mouseY = this.ValueToYinverted(panelValueForBarMouseOvered);
					} else {
						string msg = "DRAWING_CURRENT_JUMPING_STREAMING_VALUE_ON_GUTTER_SINCE_MOUSE_WENT_OUT_OF_BOUNDARIES";
						int lastIndex = this.ValueIndexLastAvailableMinusOneUnsafe;
						if (double.IsNaN(lastIndex) == false) {
							double lastValue = this.ValueGetNaNunsafe(lastIndex);
							mouseY = this.ValueToYinverted(lastValue);
						}
					}
				}
				g.DrawLine(this.ChartControl.ChartSettings.PenMousePositionTrackOnGutters, 0, mouseY, base.Width, mouseY);
			}
			if (this.GutterRightDraw) {
				int minDistanceInFontHeights = this.ThisPanelIsPricePanel ? 3 : 2;
				int minDistancePixels = minDistanceInFontHeights * this.GutterRightFontHeight_cached;
				//int panelHeightPlusSqueezers = this.PanelHeightMinusGutterBottomHeight_cached + this.PaddingVerticalSqueeze * 2;
				int panelHeightPlusSqueezers = this.PanelHeightMinusGutterBottomHeight_cached;
				double howManyLinesWillFit = panelHeightPlusSqueezers / (double)minDistancePixels;
				double gridStep = this.calculateOptimalVeritcalGridStep(this.VisibleRangeWithTwoSqueezers_cached, howManyLinesWillFit);
				double extraOverFound = this.VisibleMinMinusTopSqueezer_cached % gridStep;	// 6447 % 50 = 47
				double gridStart = this.VisibleMinMinusTopSqueezer_cached - extraOverFound + gridStep;
				int stripesCanFit = (int)(this.VisibleRangeWithTwoSqueezers_cached / gridStep);
				double gridEnd = gridStart + gridStep * stripesCanFit;
				if (gridEnd >= this.VisibleMaxPlusBottomSqueezer_cached) gridEnd -= gridStep;
				
				for (double gridPrice = gridStart; gridPrice <= gridEnd; gridPrice += gridStep) {
					int gridY = this.ValueToYinverted(gridPrice);
					g.DrawLine(this.ChartControl.ChartSettings.PenGridlinesHorizontal, 0, gridY, this.PanelWidthMinusRightPriceGutter-1, gridY);
					int labelYadjustedUp = (int)gridY - this.GutterRightFontHeightHalf_cached;
					labelYadjustedUp = this.AdjustToPanelHeight(labelYadjustedUp);
					//v1 string priceFormatted = this.ChartControl.ValueFormattedToSymbolInfoDecimalsOr5(gridPrice, this.ThisPanelIsPricePanel);
					//v2 can be price, volume or indicator
					string panelValueFormatted = this.FormatValue(gridPrice);
					int labelWidth = (int)g.MeasureString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont).Width;
					int labelXalignedRight = base.Width - this.ChartControl.ChartSettings.GutterRightPadding - labelWidth;
					g.DrawString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont,
								 this.ChartControl.ChartSettings.BrushGutterRightForeground, labelXalignedRight, labelYadjustedUp);
				}
				
				if (mouseTrack) {			// && this.moveHorizontalYprev > -1
					int mouseY = this.moveHorizontalYprev;
					if (this.mouseOver == false || this.moveHorizontalYprev == -1) {
						if (double.IsNaN(panelValueForBarMouseOvered) == false) {
							mouseY = this.ValueToYinverted(panelValueForBarMouseOvered);
						} else {
							string msg = "DRAWING_CURRENT_JUMPING_STREAMING_VALUE_ON_GUTTER_SINCE_MOUSE_WENT_OUT_OF_BOUNDARIES";
							int lastIndex = this.ValueIndexLastAvailableMinusOneUnsafe;
							if (double.IsNaN(lastIndex) == false) {
								double lastValue = this.ValueGetNaNunsafe(lastIndex);
								mouseY = this.ValueToYinverted(lastValue);
							}
						}
					}
					double mousePrice = this.YinvertedToValue(mouseY);
					int labelYadjustedUp = (int)mouseY - this.GutterRightFontHeightHalf_cached;
					labelYadjustedUp = this.AdjustToPanelHeight(labelYadjustedUp);
					//v1 string priceFormatted = this.ChartControl.ValueFormattedToSymbolInfoDecimalsOr5(mousePrice, this.ThisPanelIsPricePanel);
					//v2 can be price, volume or indicator
					string panelValueFormatted = this.FormatValue(mousePrice);
					int labelWidth = (int)g.MeasureString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont).Width;
					int labelHeight = (int)g.MeasureString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont).Height;
					int labelXalignedRight = base.Width - this.ChartControl.ChartSettings.GutterRightPadding - labelWidth;
					
					Rectangle plate = new Rectangle(labelXalignedRight, labelYadjustedUp - 2, labelWidth + 1, labelHeight + 3);
					if (base.ForeColor != Color.Empty) {
						using (SolidBrush indicatorColorBrush = new SolidBrush(base.ForeColor)) {
							g.FillRectangle(indicatorColorBrush, plate);
						}
						using (SolidBrush brushWhite = new SolidBrush(Color.White)) {
							g.DrawString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont,
										 brushWhite, labelXalignedRight, labelYadjustedUp);
						}
					} else {
						g.FillRectangle(this.ChartControl.ChartSettings.PenMousePositionTrackOnGutters.Brush, plate);
						g.DrawString(panelValueFormatted, this.ChartControl.ChartSettings.GutterRightFont,
									 this.ChartControl.ChartSettings.BrushGutterRightForeground, labelXalignedRight, labelYadjustedUp);
					}
				}
			}
			
			// before I draw vertical lines for date/time, I paint backgrounds for the bars if set from Script;
			this.renderBarBackgrounds(g);

			// now I draw vertical lines for date/time
			if (mouseTrack) {	// && this.moveHorizontalXprev > -1
				int mouseX = this.moveHorizontalXprev;
				int mouseBarIndex = this.XToBar(mouseX);
				int mouseBarX = this.BarToX(mouseBarIndex);
				if (this.mouseOver == false || this.moveHorizontalXprev == -1) {
					if (this.ChartControl.BarIndexMouseIsOverNow != -1) {
						mouseBarX = this.BarToX(this.ChartControl.BarIndexMouseIsOverNow);
					}
				}

				int mouseBarMiddleX = mouseBarX + this.BarShadowOffset;
				g.DrawLine(this.ChartControl.ChartSettings.PenMousePositionTrackOnGutters, mouseBarMiddleX, 0, mouseBarMiddleX, base.Height);
			}
			
			if (this.GutterBottomDraw == false && this.ThisPanelIsPricePanel) {
				string msg = "WHY??? this.GutterBottomDraw == false && this.ThisPanelIsPricePanel";
				Assembler.PopupException(msg);
				return;	// not initialized yet
			}
			if (this.GutterBottomDraw) {
				if (this.GutterRightFontHeight_cached <= 0) {
					string msg = "SHOULDVE_BEEN_INVOKED_EARLIER: ensureFontMetricsAreCalculated()";
					Assembler.PopupException(msg);
					return;	// not initialized yet
				}
				//this.ChartControl.ChartSettings.PenGridlinesVerticalNewDate.Width = 2f;
				int leftPadding = this.ChartControl.ChartSettings.GutterBottomPadding;
				int y = this.PanelHeightMinusGutterBottomHeight_cached + leftPadding;
				List<Rectangle> barDateLabelsAlreadyDrawn = new List<Rectangle>();
				
				// I draw the beginning of the day no matter what
				bool isIntraday_cached = this.ChartControl.Bars.IsIntraday;
				if (isIntraday_cached) {
					//int dayOpenersDrawn = 0;
					
					//if (dayOpenersDrawn == 0) {	//draw in left corner the date anyway (too much eyeballing to find the date I need)
					Bar barLeftmost = this.ChartControl.Bars[this.VisibleBarLeftExisting];
					string barLeftmostFormatted = barLeftmost.DateTimeOpen.ToString(this.ChartControl.ChartSettings.GutterBottomDateFormatDayOpener);
					int barLeftmostX = this.BarToX(this.VisibleBarLeftExisting);
					int barLeftmostWidth = (int)g.MeasureString(barLeftmostFormatted, this.ChartControl.ChartSettings.GutterBottomFont).Width;
					barLeftmostX = this.adjustToBoundariesHorizontalGutter(barLeftmostX, barLeftmostWidth);
					//g.DrawLine(this.ChartControl.ChartSettings.PenGridlinesVerticalNewDate, barLeftmostX, 0, barLeftmostX, this.PanelHeightMinusGutterBottomHeight_cached);
					g.DrawString(barLeftmostFormatted, this.ChartControl.ChartSettings.GutterBottomFont, this.ChartControl.ChartSettings.BrushGutterBottomNewDateForeground, barLeftmostX, y);
					Rectangle rectLeftmost = new Rectangle(leftPadding, y, barLeftmostWidth, this.GutterRightFontHeight_cached);
					barDateLabelsAlreadyDrawn.Add(rectLeftmost);
					//}
					
					Bar barOpenerPrevDay = this.ChartControl.Bars[this.VisibleBarRightExisting];
					int barPrevX = this.PanelWidthMinusRightPriceGutter - this.BarWidthIncludingPadding_cached;
					for (int i = this.VisibleBarRightExisting - 1; i >= this.VisibleBarLeftExisting; i--, barPrevX -= this.BarWidthIncludingPadding_cached) {
						Bar bar = this.ChartControl.Bars[i];
						if (bar.DateTimeOpen.Day == barOpenerPrevDay.DateTimeOpen.Day) continue;
						g.DrawLine(this.ChartControl.ChartSettings.PenGridlinesVerticalNewDate, barPrevX, 0, barPrevX, this.PanelHeightMinusGutterBottomHeight_cached - 1);
						
						string barDayOpener = barOpenerPrevDay.DateTimeOpen.ToString(this.ChartControl.ChartSettings.GutterBottomDateFormatDayOpener);
						int barDayOpenerWidth = (int)g.MeasureString(barDayOpener, this.ChartControl.ChartSettings.GutterBottomFont).Width;
						int barMiddleOrLeftX = barPrevX;	// - barDayOpenerWidth / 2;
						barMiddleOrLeftX = this.adjustToBoundariesHorizontalGutter(barMiddleOrLeftX, barDayOpenerWidth);
						Rectangle rect = new Rectangle(barMiddleOrLeftX, y, barDayOpenerWidth, this.GutterRightFontHeight_cached);
						bool shoulddrawBarOpenerDate = true;
						foreach (Rectangle alreadyDrawn in barDateLabelsAlreadyDrawn) {
							if (rect.IntersectsWith(alreadyDrawn) == false) continue;
							shoulddrawBarOpenerDate = false;
							break;
						}
						if (shoulddrawBarOpenerDate) {
							g.DrawString(barDayOpener, this.ChartControl.ChartSettings.GutterBottomFont, this.ChartControl.ChartSettings.BrushGutterBottomNewDateForeground, rect.Left, rect.Top);
							barDateLabelsAlreadyDrawn.Add(rect);
							//dayOpenersDrawn++;
						}
						barOpenerPrevDay = bar;
					}
				}
				
				//this.ChartControl.ChartSettings.PenGridlinesVertical.Width = 1f;
				int barMiddleX = this.PanelWidthMinusRightPriceGutter - this.BarWidthIncludingPadding_cached + this.BarShadowXoffset_cached;
				for (int i = this.VisibleBarRightExisting; i >= this.VisibleBarLeftExisting; i--, barMiddleX -= this.BarWidthIncludingPadding_cached) {
					Bar bar = this.ChartControl.Bars[i];
					if (isIntraday_cached && bar.DateTimeOpen.Minute > 0) continue;
					string dateFormatted = bar.DateTimeOpen.ToString(this.formatForBars);
					int dateFormattedWidth = (int)g.MeasureString(dateFormatted, this.ChartControl.ChartSettings.GutterBottomFont).Width;
					int xLabel = barMiddleX - dateFormattedWidth / 2;
					
					if (xLabel < 0) xLabel = 0;
					Rectangle proposal = new Rectangle(xLabel, y, dateFormattedWidth, this.GutterRightFontHeight_cached);
					bool tooCrowded = false;
					foreach (Rectangle alreadyDrawn in barDateLabelsAlreadyDrawn) {
						if (proposal.IntersectsWith(alreadyDrawn) == false) continue;
						tooCrowded = true;
						break;
					}
					if (tooCrowded) continue;
					g.DrawLine(this.ChartControl.ChartSettings.PenGridlinesVertical, barMiddleX, 0, barMiddleX, this.PanelHeightMinusGutterBottomHeight_cached - 1);
					g.DrawString(dateFormatted, this.ChartControl.ChartSettings.GutterBottomFont, this.ChartControl.ChartSettings.BrushGutterBottomForeground, xLabel, y);
					barDateLabelsAlreadyDrawn.Add(proposal);
				}

				if (mouseTrack) {	// && this.moveHorizontalXprev > -1
					int mouseX = this.moveHorizontalXprev;
					int mouseBarIndex = this.XToBar(mouseX);
					int mouseBarX = this.BarToX(mouseBarIndex);
					int mouseBarMiddleX = mouseBarX + this.BarShadowOffset;
					
					Bar mouseBar = this.ChartControl.Bars[mouseBarIndex];
					if (null != mouseBar) {
						string dateFormatted = mouseBar.DateTimeOpen.ToString(this.formatForBars);
						int dateFormattedWidth = (int)g.MeasureString(dateFormatted, this.ChartControl.ChartSettings.GutterBottomFont).Width;
						int xLabel = mouseBarMiddleX - dateFormattedWidth / 2;
						
						if (xLabel < 0) xLabel = 0;
						Rectangle plate = new Rectangle(xLabel - 2, y, dateFormattedWidth + 2, this.GutterRightFontHeight_cached);
						g.FillRectangle(this.ChartControl.ChartSettings.PenMousePositionTrackOnGutters.Brush, plate);
						g.DrawString(dateFormatted, this.ChartControl.ChartSettings.GutterBottomFont, this.ChartControl.ChartSettings.BrushGutterBottomForeground, xLabel, y);
					}
				}
			}
		}
		int dateMeasuredWidthFormattedByBarScale(Graphics g, DateTime value) {
			string valueFormatted = value.ToString(this.formatForBars);
			int ret = (int)g.MeasureString(valueFormatted, this.ChartControl.ChartSettings.GutterBottomFont).Width;
			return ret;
		}

		// http://stackoverflow.com/questions/361681/algorithm-for-nice-grid-line-intervals-on-a-graph
		double calculateOptimalVeritcalGridStep(double range, double targetSteps) {
			// calculate an initial guess at step size
			double tempStep = range / (double)targetSteps;

			// get the magnitude of the step size
			double mag = (float)Math.Floor(Math.Log10(tempStep));
			double magPow = (float)Math.Pow(10, mag);

			// calculate most significant digit of the new step size
			if (magPow == -0.5) {
				string msg = "DONT_INVOKE_ME_FOR_PANEL_HEIGHT_ZERO";
				Debugger.Break();
				return 0;
			}
			
			double magMsd = 0;
			try {
				magMsd = (int)(tempStep / magPow + 0.5);
			} catch (Exception ex) {
				Debugger.Break();
				return 10;
			}
			// promote the MSD to either 1, 2, or 5
			if (magMsd > 5.0) magMsd = 10.0f;
			else if (magMsd > 2.0) magMsd = 5.0f;
			else if (magMsd > 1.0) magMsd = 2.0f;

			return magMsd * magPow;
		}
		protected void RenderBarHistogram(Graphics graphics, int barX, int barYVolumeInverted, bool fillDownCandleBody) {
			int histogramBarHeight = this.PanelHeightMinusGutterBottomHeight_cached - barYVolumeInverted;		// height is measured DOWN the screen from candleBodyInverted.Y, not UP
			if (histogramBarHeight < 0)  Debugger.Break();
			if (histogramBarHeight == 0) return;	//candleBodyInverted.Height = 1;

			Rectangle histogramBarInverted = default(Rectangle);
			histogramBarInverted.X = barX;
			histogramBarInverted.Width = this.BarWidthMinusRightPadding_cached;
			histogramBarInverted.Y = barYVolumeInverted;					// drawing down, since Y grows down the screen from left upper corner (0:0)
			histogramBarInverted.Height = histogramBarHeight;		// height is measured DOWN the screen from candleBodyInverted.Y, not UP

			var brushDown = (fillDownCandleBody)
				? this.ChartControl.ChartSettings.BrushVolumeBarDown
				: this.ChartControl.ChartSettings.BrushVolumeBarUp;
			//if (fillDownCandleBody) histogramBarInverted.Width--;	// SYNC_WITH_RenderBarCandle drawing using a pen produces 1px narrower rectangle that drawing using a brush???...
			if (this.ForeColor != Color.Empty) {
				using (SolidBrush brushNonVolume = new SolidBrush(this.ForeColor)) {
					graphics.FillRectangle(brushNonVolume, histogramBarInverted);
				}
			} else {
				graphics.FillRectangle(brushDown, histogramBarInverted);
			}
		}
		protected void RenderBarCandle(Graphics graphics, int barX, int barYOpenInverted, int barYHighInverted, int barYLowInverted, int barYCloseInverted, bool fillDownCandleBody) {
			int candleBodyLower = barYOpenInverted;	// assuming it is a white candle (rising price)
			int candleBodyHigher = barYCloseInverted;
			if (barYOpenInverted > barYCloseInverted) {
				if (fillDownCandleBody == true) {		//FIXIT: should be false
					Debugger.Break();
				}
				candleBodyLower = barYCloseInverted;	// nope it's a black candle (falling price)
				candleBodyHigher = barYOpenInverted;
			}
			
			int candleBodyHeight = candleBodyHigher - candleBodyLower;		// height is measured DOWN the screen from candleBodyInverted.Y, not UP
			if (candleBodyHeight < 0) Debugger.Break();
			if (candleBodyHeight == 0) candleBodyHeight = 1;

			Rectangle candleBodyInverted = default(Rectangle);
			candleBodyInverted.X = barX;
			candleBodyInverted.Width = this.BarWidthMinusRightPadding_cached;
			candleBodyInverted.Y = candleBodyLower;					// drawing down, since Y grows down the screen from left upper corner (0:0)
			candleBodyInverted.Height = candleBodyHeight;

			int shadowX = barX + this.BarShadowXoffset_cached;
			if (fillDownCandleBody) {
				var brushDown = this.ChartControl.ChartSettings.BrushPriceBarDown;
				var penDown = this.ChartControl.ChartSettings.PenPriceBarDown;
				graphics.FillRectangle(brushDown, candleBodyInverted);
				graphics.DrawLine(penDown, shadowX, barYHighInverted, shadowX, barYLowInverted);
			} else {
				var brushUp = this.ChartControl.ChartSettings.BrushPriceBarUp;
				var penUp = this.ChartControl.ChartSettings.PenPriceBarUp;
				candleBodyInverted.Width--;	// drawing using a pen produces 1px narrower rectangle that drawing using a brush???...
				graphics.DrawRectangle(penUp, candleBodyInverted);
				graphics.DrawLine(penUp, shadowX, barYHighInverted, shadowX, candleBodyInverted.Top);
				graphics.DrawLine(penUp, shadowX, barYLowInverted, shadowX, candleBodyInverted.Bottom);
			}
		}

		protected void RenderIndicators(Graphics graphics) {
			// BT_ONSLIDERS_OFF>BT_NOW>SWITCH_SYMBOL=>INDICATOR.OWNVALUES.COUNT=0=>DONT_RENDER_INDICATORS_BUT_RENDER_BARS
			bool skipPaintingIndicatorsBacktestDidntRunOrIncomplete = this.ChartControl.ScriptExecutorObjects.IndicatorsAllHaveNoOwnValues;
			if (skipPaintingIndicatorsBacktestDidntRunOrIncomplete) {
				return;
			}
			// ALREADY_CHECKED_FOR_NULL_OR_EMPTY
			Dictionary<string, Indicator> indicators = this.ChartControl.ScriptExecutorObjects.Indicators;

			int barX = this.ChartControl.ChartWidthMinusGutterRightPrice;
			// i > this.VisibleBarLeft_cached is enough because Indicator.Draw() takes previous bar
			for (int i = this.VisibleBarRight_cached; i > this.VisibleBarLeft_cached; i--) {
				Bar bar = this.ChartControl.Bars[i];

//				int barYHighInverted = this.ValueToYinverted(bar.High);
//				int barYLowInverted = this.ValueToYinverted(bar.Low);
//
//				int candleBodyHeight = barYLowInverted - barYHighInverted;		// height is measured DOWN the screen from candleBodyInverted.Y, not UP
//				if (candleBodyHeight < 0) Debugger.Break();
//				if (candleBodyHeight == 0) candleBodyHeight = 1;
//
//				Rectangle candleBodyInverted = default(Rectangle);
//				candleBodyInverted.X = barX;
//				candleBodyInverted.Width = this.BarWidthMinusRightPadding_cached;
//				candleBodyInverted.Y = barYHighInverted;					// drawing down, since Y grows down the screen from left upper corner (0:0)
//				candleBodyInverted.Height = candleBodyHeight;

				barX -= this.BarWidthIncludingPadding_cached;
				foreach (Indicator indicator in indicators.Values) {
					if (indicator.HostPanelForIndicator != this) continue;
					if (bar.ParentBarsIndex <= indicator.FirstValidBarIndex) continue;
//					bool indicatorLegDrawn = indicator.DrawValueEntryPoint(graphics, bar, candleBodyInverted);
					bool indicatorLegDrawn = indicator.DrawValueEntryPoint(graphics, bar);
				}
			}
			foreach (Indicator indicator in indicators.Values) {
				if (indicator.HostPanelForIndicator != this) continue;
				if (indicator.Executor == null) {							// indicator.BarsEffective will throw if indicator.Executor==null
					#if DEBUG
					Debugger.Break();
					#endif
					continue;
				}
				if (indicator.BarsEffective == null) {
					string msg = "DONT_RUN_BACKTESTER_BEFORE_BARS_ARE_LOADED /RenderIndicators()";
					Assembler.PopupException(msg);
					#if DEBUG
					Debugger.Break();
					#endif
					continue;
				}
				if (indicator.BarsEffective.BarLast == null) continue;
				
				// UNNECESSARY_BUT_HELPS_CATCH_BUGS begin
				if (indicator.DotsDrawnForCurrentSlidingWindow <= 0) {
					Bar barLeft = this.ChartControl.Bars[this.VisibleBarLeft_cached];
					string dateRq = barLeft.DateTimeOpen.ToString(Assembler.DateTimeFormatIndicatorHasNoValuesFor);
					if (indicator.BarsEffective.BarFirst == null) {	// happens after I edited DataSource and removed ",Sun" from DaysMarketOpen
						#if DEBUG
						Debugger.Break();
						#endif
						string msg2 = "SKIPPING_RENDERING_INDICATOR_TITLES for indicator[" + indicator + "].BarsEffective[" + indicator.BarsEffective + "].BarFirst=null";
						Assembler.PopupException(msg2);
						continue;
					}
					string dateFirst = indicator.BarsEffective.BarFirst.DateTimeOpen.ToString(Assembler.DateTimeFormatIndicatorHasNoValuesFor);
					string dateLast = indicator.BarsEffective.BarLast.DateTimeOpen.ToString(Assembler.DateTimeFormatIndicatorHasNoValuesFor);
					//TOO_NOISY_SEE_TOOLTIP_PRICE msg += " (" + dateFirst + ")..(" + dateLast + ") <> barRq[" + dateRq + "]";
				}
				// UNNECESSARY_BUT_HELPS_CATCH_BUGS end
				
				if (this.ThisPanelIsIndicatorPanel) return;
				string indicatorLabel = indicator.NameWithParameters;
				this.DrawLabelOnNextLine(graphics, indicatorLabel, null, indicator.LineColor, Color.Empty, true);
			}
		}
	}
}
