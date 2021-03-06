using System;
using System.Diagnostics;
using System.Text;

using Newtonsoft.Json;

namespace Sq1.Core.DataTypes {
	public class Quote {
		[JsonIgnore]	public const int	IntraBarSernoShiftForGeneratedTowardsPendingFill = 100000;

		[JsonProperty]	public string		Symbol;
		[JsonProperty]	public string		SymbolClass;
		[JsonProperty]	public string		Source;
		[JsonProperty]	public DateTime		ServerTime;
		[JsonProperty]	public DateTime		LocalTimeCreated	{ get; protected set; }

		[JsonProperty]	public double		Bid;
		[JsonProperty]	public double		Ask;
		[JsonProperty]	public double		Size;
		[JsonIgnore]	public BidOrAsk		ItriggeredFillAtBidOrAsk;
		
		[JsonIgnore]	public int			IntraBarSerno;
		[JsonProperty]	public int			AbsnoPerSymbol;

		[JsonIgnore]	public Bar			ParentBarStreaming	{ get; protected set; }
		[JsonIgnore]	public bool			HasParentBar		{ get { return this.ParentBarStreaming != null; } }
		[JsonProperty]	public string		ParentBarIdent		{ get { return (this.HasParentBar) ? this.ParentBarStreaming.ParentBarsIdent : "NO_PARENT_BAR"; } }

		[JsonProperty]	public BidOrAsk		LastDealBidOrAsk;
		[JsonProperty]	public double		LastDealPrice		{ get {		//WRITTEN_ONLY_BY_QUOTE_GENERATOR MADE_READONLY_COZ_WRITING_IS_A_WRONG_CONCEPT
				if (this.LastDealBidOrAsk == BidOrAsk.UNKNOWN) return double.NaN;
				return (this.LastDealBidOrAsk == BidOrAsk.Bid) ? this.Bid : this.Ask;
			} }
		[JsonProperty]	public double		Spread				{ get { return this.Ask - this.Bid; } }


		protected Quote() {	// make it proteted and use it when you'll need to super-modify a quote in StreamingProvider-derived 
			ServerTime = DateTime.MinValue;
			//Absno = ++AbsnoStaticCounterForAllSymbolsUseless;
			AbsnoPerSymbol = -1;	// QUOTE_ABSNO_MUST_BE_SEQUENTIAL_PER_SYMBOL INITIALIZED_IN_STREAMING_PROVIDER
			IntraBarSerno = -1;
			Bid = double.NaN;
			Ask = double.NaN;
			Size = -1;
			ItriggeredFillAtBidOrAsk = BidOrAsk.UNKNOWN;
			LastDealBidOrAsk = BidOrAsk.UNKNOWN;
		}
		public Quote(DateTime localTimeEqualsToServerTimeForGenerated) : this() {
			// PROFILER_SAID_DATETIME.NOW_IS_SLOW__I_DONT_NEED_IT_FOR_BACKTEST_ANYWAY
			LocalTimeCreated = (localTimeEqualsToServerTimeForGenerated != DateTime.MinValue)
				? localTimeEqualsToServerTimeForGenerated : DateTime.Now;
		}
		public void SetParentBarStreaming(Bar parentBar) {
			if (this.Symbol != parentBar.Symbol) {
				string msg = "SYMBOL_MISMATCH__CANT_SET_PARENT_BAR_FOR_QUOTE quote.Symbol[" + this.Symbol + "] != parentBar.Symbol[" + parentBar.Symbol + "]";
				Assembler.PopupException(msg);
			}
			this.ParentBarStreaming = parentBar;
		}

		#region SORRY_FOR_THE_MESS__I_NEED_TO_DERIVE_IDENTICAL_ONLY_FOR_GENERATED__IF_YOU_NEED_IT_IN_BASE_QUOTE_MOVE_IT_THERE
		public Quote Clone() {
			return (Quote)this.MemberwiseClone();
		}
		//public Quote DeriveIdenticalButFresh() {
		//	Quote identicalButFresh = new Quote();
		//	identicalButFresh.Symbol = this.Symbol;
		//	identicalButFresh.SymbolClass = this.SymbolClass;
		//	identicalButFresh.Source = this.Source;
		//	identicalButFresh.ServerTime = this.ServerTime.AddMilliseconds(911);
		//	identicalButFresh.LocalTimeCreatedMillis = this.LocalTimeCreatedMillis.AddMilliseconds(911);
		//	identicalButFresh.PriceLastDeal = this.PriceLastDeal;
		//	identicalButFresh.Bid = this.Bid;
		//	identicalButFresh.Ask = this.Ask;
		//	identicalButFresh.Size = this.Size;
		//	identicalButFresh.IntraBarSerno = this.IntraBarSerno + Quote.IntraBarSernoShiftForGeneratedTowardsPendingFill;
		//	identicalButFresh.ParentStreamingBar = this.ParentStreamingBar;
		//	return identicalButFresh;
		//}
		#endregion

		//public override string ToString() {
		//    string ret = "#" + this.IntraBarSerno + "/" + this.Absno + " " + this.Symbol;
		//    //ret += " bid{" + Math.Round(this.Bid, 3) + "-" + Math.Round(this.Ask, 3) + "}ask"
		//    //ret += " size{" + this.Size + "@" + Math.Round(this.PriceLastDeal, 3) + "}lastDeal";
		//    ret += " bid{" + this.Bid + "-" + this.Ask + "}ask size{" + this.Size + "@" + this.LastDealPrice + "}lastDeal";
		//    if (ServerTime != null) ret += " SERVER[" + ServerTime.ToString("HH:mm:ss.fff") + "]";
		//    ret += "[" + LocalTimeCreatedMillis.ToString("HH:mm:ss.fff") + "]LOCAL";
		//    if (string.IsNullOrEmpty(this.Source) == false) ret += " " + Source;
		//    ret += " STR:" + this.ParentBarIdent;
		//    return ret;
		//}
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append("#");
			sb.Append(this.IntraBarSerno);
			sb.Append("/");
			sb.Append(this.AbsnoPerSymbol);
			sb.Append(" ");
			sb.Append(this.Symbol);
			sb.Append(" bid{");
			sb.Append(this.Bid);
			sb.Append("-");
			sb.Append(this.Ask);
			sb.Append("}ask size{");
			sb.Append(this.Size);
			sb.Append("@");
			sb.Append(this.LastDealPrice);
			sb.Append("}lastDeal");
			sb.Append(this.LastDealBidOrAsk);
			sb.Append(" ");
			bool timesAreDifferent = true;
			if (this.ServerTime != null) {
				if (this.ServerTime == this.LocalTimeCreated) {
					timesAreDifferent = false;
				}
				if (timesAreDifferent == true) {
					sb.Append(" SERVER[");
					sb.Append(this.ServerTime.ToString("HH:mm:ss.fff"));
					sb.Append("]");
				}
			}
			sb.Append("[");
			sb.Append(this.LocalTimeCreated.ToString("HH:mm:ss.fff"));
			sb.Append("]");
			if (timesAreDifferent == true) {
				sb.Append("LOCAL");
			}
			sb.Append(" ");
			if (string.IsNullOrEmpty(this.Source) == false) sb.Append(this.Source);
			sb.Append("STR:");
			sb.Append(this.ParentBarIdent);
			return sb.ToString();
		}
		//public string ToStringShort() {
		//    string ret = "#" + this.IntraBarSerno + "/" + this.Absno + " " + this.Symbol
		//        + " bid{" + this.Bid + "-" + this.Ask + "}ask size{" + this.Size + "}"
		//        + ": " + this.ParentBarIdent;
		//    return ret;
		//}
		public string ToStringShort() {
			StringBuilder sb = new StringBuilder();
			sb.Append("#");
			sb.Append(this.IntraBarSerno);
			sb.Append("/");
			sb.Append(this.AbsnoPerSymbol);
			sb.Append(" ");
			sb.Append(this.Symbol);
			sb.Append(" bid{");
			sb.Append(this.Bid);
			sb.Append("-");
			sb.Append(this.Ask);
			sb.Append("}ask size{");
			sb.Append(this.Size);
			sb.Append("}");
			sb.Append(": ");
			sb.Append(this.ParentBarIdent);
			return sb.ToString();
		}
		public bool SameBidAsk(Quote other) {
			return (this.Bid == other.Bid && this.Ask == other.Ask);
		}
		public bool PriceBetweenBidAsk(double price) {
			//return (price >= this.Bid && price <= this.Ask);
			double diffUp = this.Ask - price;						// WTF 1760.2-1706.2=-2.27E-13
			double diffDn = price - this.Bid;
			bool ret = diffUp >= 0 && diffDn >= 0;

			if (ret == false) {
				//Debugger.Break();									// WTF 1760.2-1706.2=-2.27E-13
				double diffUpRounded = Math.Round(diffUp, 5);		// WTF 1760.2-1706.2=-2.27E-13
				double diffDnRounded = Math.Round(diffDn, 5);
				ret = diffUpRounded >= 0 && diffDnRounded >= 0;
	
				// moved to post-fill check upstack:
				// if (filled == 1 && this.fillOutsideQuoteSpreadParanoidCheckThrow == true) {alert.IsFilledOutsideQuote_DEBUG_CHECK;alert.IsFilledOutsideBarSnapshotFrozen_DEBUG_CHECK;}
				//if (ret == false) {
				//    #if DEBUG
				//    Debugger.Break();	// ENABLE_BREAK_UPSTACK_IF_YOU_COMMENT_IT_OUT_HERE
				//    #endif
				//}
			}
			return ret;
		}
	}
}