﻿using System;
using System.Drawing;

using BrightIdeasSoftware;
using Sq1.Core.Indicators;
using Sq1.Core.StrategyBase;

namespace Sq1.Widgets.Optimization {
	public partial class OptimizerControl {
		Color colorBackgroundRed;
		Color colorBackgroundGreen;
		
		void olvBacktests_FormatRow(object sender, FormatRowEventArgs e) {
			SystemPerformance systemPerformance = e.Model as SystemPerformance;
			if (systemPerformance == null) return;
			e.Item.BackColor = (systemPerformance.SlicesShortAndLong.NetProfitForClosedPositionsBoth > 0.0) ? this.colorBackgroundGreen : this.colorBackgroundRed;
			//if (value == ethalonRedIfLessBlueIfGreater) return this.ForeColor;
			//return (value > ethalonRedIfLessBlueIfGreater) ? Color.Blue : Color.Red;
		}
		void objectListViewCustomizeColors() {
			//if (this.snap.Colorify) {
				this.colorBackgroundRed = Color.FromArgb(255, 230, 230);
				this.colorBackgroundGreen = Color.FromArgb(230, 255, 230);
				this.olvBacktests.UseCellFormatEvents = true;
				this.olvBacktests.FormatRow += new EventHandler<FormatRowEventArgs>(olvBacktests_FormatRow);
			//} else {
			//	this.olvPositions.UseCellFormatEvents = false;
			//	this.olvPositions.FormatRow -= new EventHandler<FormatRowEventArgs>(olvPositions_FormatRow);
			//}
		}
		void objectListViewCustomize() {
			this.objectListViewCustomizeColors();
			this.olvcSerno.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcSerno.AspectGetter: systemPerformance=null";
				return (this.backtests.IndexOf(systemPerformance) + 1).ToString();
			};
			this.olvcNetProfit.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcNetProfit.AspectGetter: systemPerformance=null";
				string format = systemPerformance.Bars.SymbolInfo.FormatPrice;
				return systemPerformance.SlicesShortAndLong.NetProfitForClosedPositionsBoth.ToString(format);
			};
			this.olvcTotalTrades.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcTotalTrades.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.PositionsCountBoth.ToString();
			};
			this.olvcAverageProfit.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcAverageProfit.AspectGetter: systemPerformance=null";
				string format = systemPerformance.Bars.SymbolInfo.FormatPrice;
				return systemPerformance.SlicesShortAndLong.AvgProfitBoth.ToString(format);
			};
			this.olvcNetProfit.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcNetProfit.AspectGetter: systemPerformance=null";
				string format = systemPerformance.Bars.SymbolInfo.FormatPrice;
				return systemPerformance.SlicesShortAndLong.NetProfitForClosedPositionsBoth.ToString(format);
			};
			this.olvcWinLoss.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcWinLoss.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.WinLossRatio.ToString();
			};
			this.olvcProfitFactor.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcProfitFactor.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.ProfitFactor.ToString();
			};
			this.olvcRecoveryFactor.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcRecoveryFactor.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.RecoveryFactor.ToString();
			};
			this.olvcMaxDrawdown.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcMaxDrawdown.AspectGetter: systemPerformance=null";
				string format = systemPerformance.Bars.SymbolInfo.FormatPrice;
				return systemPerformance.SlicesShortAndLong.MaxDrawDown.ToString(format);
			};
			this.olvcMaxConsecutiveWinners.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcMaxConsecutiveWinners.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.MaxConsecWinners.ToString();
			};
			this.olvcMaxConsecutiveLosers.AspectGetter = delegate(object o) {
				SystemPerformance systemPerformance = o as SystemPerformance;
				if (systemPerformance == null) return "olvcMaxConsecutiveLosers.AspectGetter: systemPerformance=null";
				return systemPerformance.SlicesShortAndLong.MaxConsecLosers.ToString();
			};
			
			foreach (OLVColumn colDynParam in this.columnsDynParam) {
				//v1
				//AspectGetterDelegateWrapper individualDelgateForEachColumn = new AspectGetterDelegateWrapper(colDynParam.Name);
				//colDynParam.AspectGetter = individualDelgateForEachColumn
				//string colDynParamNameStatic = colDynParam.Name;
				//colDynParam.AspectGetter = new AspectGetterDelegate(object o) {
				//    string colDynParamNameStatic2 = colDynParam.Name;
				//    if (colDynParamNameStatic2 != colDynParamNameStatic) {
				//        //Debugger.Break();	// THIS_IS_WHY_I_HATE_LAMBDAS
				//    }

				//    SystemPerformance systemPerformance = o as SystemPerformance;
				//    if (systemPerformance == null) return colDynParamNameStatic + ".AspectGetter: systemPerformance=null";
				//    if (systemPerformance.ScriptAndIndicatorParameterClonesByName.ContainsKey(colDynParamNameStatic) == false) {
				//        return colDynParamNameStatic + ".AspectGetter: !systemPerformance.ScriptAndIndicatorParametersByName[" + colDynParamNameStatic + "]";
				//    }
				//    IndicatorParameter param = systemPerformance.ScriptAndIndicatorParameterClonesByName[colDynParamNameStatic];
				//    return param.ValueCurrent.ToString();
				//};
				// v2: cool but it didn't help
				AspectGetterDelegateWrapper individualDelgateForEachColumn = new AspectGetterDelegateWrapper(colDynParam.Name);
				colDynParam.AspectGetter = (AspectGetterDelegate) individualDelgateForEachColumn.AspectGetterDelegateImplementor;
			}
		}
	}
	class AspectGetterDelegateWrapper {
		string colDynParamName;
		public AspectGetterDelegateWrapper(string colDynParamName) {
			this.colDynParamName = colDynParamName;
		}
		public string AspectGetterDelegateImplementor(object o) {
			SystemPerformance systemPerformance = o as SystemPerformance;
			if (systemPerformance == null) return colDynParamName + ".AspectGetter: systemPerformance=null";
			if (systemPerformance.ScriptAndIndicatorParameterClonesByName.ContainsKey(colDynParamName) == false) {
				return colDynParamName + ".AspectGetter: !systemPerformance.ScriptAndIndicatorParametersByName[" + colDynParamName + "]";
			}
			IndicatorParameter param = systemPerformance.ScriptAndIndicatorParameterClonesByName[colDynParamName];
			return param.ValueCurrent.ToString();
		}
	}
}
