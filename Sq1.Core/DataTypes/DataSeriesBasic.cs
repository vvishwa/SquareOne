using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sq1.Core.DataTypes {
	public class DataSeriesBasic {
		public string Description;
		List<double> doubleValues;
		public virtual IList<double> Values { get { return this.doubleValues; } }
		public virtual int StreamingIndex { get { return this.doubleValues.Count - 1; } }
		public virtual double StreamingValue {
			get { return (this.doubleValues.Count == 0) ? double.NaN : this[this.doubleValues.Count - 1]; }
			set {
				if (this.doubleValues.Count == 0) {
					string msg = "USE_APPEND_BECAUSE_DOUBLE_VALUES_ARE_EMPTY_NOW; STREAMING_VALUE_IS_SIMPLY_LAST_ELEMENT";
					#if DEBUG
					Debugger.Break();
					#endif
					throw new Exception(msg);
				}
				this[this.doubleValues.Count - 1] = value;
			}
		}
		public virtual int Count { get { return this.doubleValues.Count; } }
		public virtual int Capacity {
			get { return this.doubleValues.Capacity; }
			set { this.doubleValues.Capacity = value; }
		}
		public DataSeriesBasic() {
			this.doubleValues = new List<double>();
			#if DEBUG
			if (double.IsNaN(this.StreamingValue) == false) {
				Debugger.Break();
			}
			#endif
		}
		public DataSeriesBasic(string description) : this() {
			this.Description = description;
		}
		public virtual double this[int barIndex] {
			get {
				if (barIndex < 0 || barIndex > this.doubleValues.Count - 1) {
					string msg = this.Description + "[" + barIndex + "] is out of bounds: " 
						+ this.Description + ".Count=" + this.doubleValues.Count;
					throw new ArgumentOutOfRangeException(msg);
				}
				if (barIndex == this.doubleValues.Count) {
					string msg = "DEPRECATED_NOTATION this[this.Count]_get; STREAMING_VALUE_IS_NOW_AT_this[this.Count-1]";
					Debugger.Break();
					throw new Exception(msg);
					//return this.doubleValues[barIndex - 1];
				}
				return this.doubleValues[barIndex];
			}
			set {
				if (barIndex < 0 || barIndex > this.Count) {
					string msg = "[" + barIndex + "] is out of bounds: " + this;
					throw new ArgumentOutOfRangeException(msg);
				}
				if (barIndex == this.doubleValues.Count) {
					string msg = "DEPRECATED_NOTATION this[this.Count]_set; STREAMING_VALUE_IS_NOW_AT_this[this.Count-1]";
					Debugger.Break();
					throw new Exception(msg);
					//this.StreamingValue = value;
					//return;
				}
				this.doubleValues[barIndex] = value;
			}
		}
		public virtual void Clear() {
			this.doubleValues.Clear();
		}
		public virtual void Append(double value) {
			this.doubleValues.Add(value);
		}
		public override string ToString() {
			string ret = this.Count + "doubleValues";
			if (this.Count > 0) {
				ret += " last=[" + this.StreamingValue + "] @[" + this.StreamingIndex + "]";
			}
			if (string.IsNullOrEmpty(this.Description) == false) ret += "/" + this.Description;
			return ret;
		}
	
		public bool IsTrendingUpAtBarIndex(int barIndex) {
			if (barIndex <= 0) return false;
			if (this.Count < 2) return false;
			if (this[barIndex] > this[barIndex - 1]) return true;
			return false;
		}
		public bool IsTrendingDownAtBarIndex(int barIndex) {
			if (barIndex <= 0) return false;
			if (this.Count < 2) return false;
			if (this[barIndex] < this[barIndex - 1]) return true;
			return false;
		}
		public bool TurnsDownAtBarIndex(int barIndex) {
			if (barIndex <= 0) return false;
			if (this.Count < 2) return false;
			if (this[barIndex] >= this[barIndex - 1]) return false;
			for (int i = barIndex - 1 ; i >= 1 ; i--) {
				if (this[i] > this[i - 1]) return true;
				if (this[i] < this[i - 1]) return false;
			}
			return false;
		}
		public bool TurnsUpAtBarIndex(int barIndex) {
			if (barIndex <= 0) return false;
			if (this.Count < 2) return false;
			if (this[barIndex] <= this[barIndex - 1]) return false;
			for (int i = barIndex - 1 ; i >= 1 ; i--) {
				if (this[i] < this[i - 1]) return true;
				if (this[i] > this[i - 1]) return false;
			}
			return false;
		}
		#region extracted from ChartControl.DynamicProperties.cs, from VisiblePriceMin,Max,VisibleVolumeMin,Max; re-used in PanelIndicator.ValueMin,Max  
		public double MinValueBetweenIndexesDoubleMaxValueUnsafe(int indexLeft, int indexRight) {
			double ret = double.MaxValue;
			int indexMin = Math.Min(indexLeft, indexRight);
			int indexMax = Math.Max(indexLeft, indexRight);
			for (int i = indexMax; i >= indexMin; i--) {
				if (i >= this.Count) {	// we want to display 0..64, but Bars has only 10 bars inside
					string msg = "YOU_SHOULD_INVOKE_SyncHorizontalScrollToBarsCount_PRIOR_TO_RENDERING_I_DONT_KNOW_ITS_NOT_SYNCED_AFTER_ChartControl.Initialize(Bars)";
					#if DEBUG
					Debugger.Break();
					#endif
					Assembler.PopupException("VisibleVolumeMin(): " + msg);
					continue;
				}
				double barCanBeStreamingWithNaNs = this[i];		// this[int] is virtual, for Indicator.OwnValuesCalculated this[int] is located in DataSeriesTimeBased.cs
				if (double.IsNaN(barCanBeStreamingWithNaNs)) continue;
				if (barCanBeStreamingWithNaNs < ret) ret = barCanBeStreamingWithNaNs;
			}
			#if DEBUG
			if (ret == double.MaxValue) {
				// IndicatorAtrBand.OwnValuesCalculated are all NaNs; DONT_NORMALIZE_VERTICALLY_INDICATORS_DRAWING_ON_PRICE_PANEL
				//Debugger.Break();
			}
			#endif
			return ret;
		}
		public double MaxValueBetweenIndexesDoubleMinValueUnsafe(int indexLeft, int indexRight) {
			double ret = double.MinValue;
			int indexMin = Math.Min(indexLeft, indexRight);
			int indexMax = Math.Max(indexLeft, indexRight);
			for (int i = indexMax; i >= indexMin; i--) {
				if (i >= this.Count) {	// we want to display 0..64, but Bars has only 10 bars inside
					string msg = "YOU_SHOULD_INVOKE_SyncHorizontalScrollToBarsCount_PRIOR_TO_RENDERING_I_DONT_KNOW_ITS_NOT_SYNCED_AFTER_ChartControl.Initialize(Bars)";
					#if DEBUG
					Debugger.Break();
					#endif
					Assembler.PopupException("VisibleVolumeMax(): " + msg);
					continue;
				}
				double barCanBeStreamingWithNaNs = this[i];		// this[int] is virtual, for Indicator.OwnValuesCalculated this[int] is located in DataSeriesTimeBased.cs
				if (double.IsNaN(barCanBeStreamingWithNaNs)) continue;
				if (barCanBeStreamingWithNaNs > ret) ret = barCanBeStreamingWithNaNs;
			}
			#if DEBUG
			if (ret == double.MinValue) {
				//Debugger.Break();
			}
			#endif
			return ret;
		}
		#endregion
	}
}
