﻿using System;
using Sq1.Core.DataTypes;

namespace Sq1.Adapters.Quik {
	public class QuoteQuik : Quote {
		public double FortsDepositBuy;
		public double FortsDepositSell;

		public double FortsPriceMax;
		public double FortsPriceMin;

		public QuoteQuik(DateTime quoteDate) : base(quoteDate) {
		}

		public void EnrichFromStreamingDataSnapshotQuik(StreamingDataSnapshotQuik quikStreamingDataSnapshot) {
			this.FortsDepositBuy = quikStreamingDataSnapshot.FortsGetDepositBuyForSymbol(base.Symbol);
			this.FortsDepositSell = quikStreamingDataSnapshot.FortsGetDepositSellForSymbol(base.Symbol);
			this.FortsPriceMin = quikStreamingDataSnapshot.FortsGetPriceMinForSymbol(base.Symbol);
			this.FortsPriceMax = quikStreamingDataSnapshot.FortsGetPriceMaxForSymbol(base.Symbol);
		}
		public static QuoteQuik SafeUpcast(Quote quote) {
			if (quote is QuoteQuik == false) {
				string msg = "Should be of a type Sq1.Adapters.Quik.QuoteQuik instead of Sq1.Core.DataTypes.Quote: "
					+ quote;
				throw new Exception(msg);
			}
			return quote as QuoteQuik;
		}
	}
}