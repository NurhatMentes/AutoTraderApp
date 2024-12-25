using AutoTraderApp.Domain.Entities;
using System.Globalization;

namespace AutoTraderApp.Core.Utilities.Generator
{
    public static class StrategyScriptGenerator
    {
        public static string GenerateScript(Strategy strategy, int quantity, string symbol)
        {
            return $@"
//@version=6
strategy(""{strategy.StrategyName}"", overlay=true, default_qty_type=strategy.percent_of_equity, default_qty_value=1)

// === Inputs ===
atr_length = input.int({strategy.AtrLength}, title=""ATR Length"")
bollinger_length = input.int({strategy.BollingerLength}, title=""Bollinger Bands Length"")
bollinger_mult = input.float({strategy.BollingerMultiplier}, title=""Bollinger Bands Multiplier"")
dmi_length = input.int({strategy.DmiLength}, title=""DMI Length"")
adx_smoothing = input.int({strategy.AdxSmoothing}, title=""ADX Smoothing"") 
adx_threshold = input.float({strategy.AdxThreshold}, title=""ADX Threshold"")
rsi_length = input.int({strategy.RsiLength}, title=""RSI Length"")
rsi_upper = input.float({strategy.RsiUpper}, title=""RSI Overbought Level"")
rsi_lower = input.float({strategy.RsiLower}, title=""RSI Oversold Level"")
stoch_rsi_length = input.int({strategy.StochRsiLength}, title=""Stoch RSI Length"")
stoch_rsi_upper = input.float({strategy.StochRsiUpper}, title=""Stoch RSI Overbought Level"")
stoch_rsi_lower = input.float({strategy.StochRsiLower}, title=""Stoch RSI Oversold Level"")

// === Calculations ===
atr = ta.atr(atr_length)
basis = ta.sma(close, bollinger_length)
upper_band = basis + bollinger_mult * ta.stdev(close, bollinger_length)
lower_band = basis - bollinger_mult * ta.stdev(close, bollinger_length)
[plus_di, minus_di, adx] = ta.dmi(dmi_length, adxSmoothing=adx_smoothing)

rsi_val = ta.rsi(close, rsi_length)

// Stochastic RSI calculation
rsi_high = ta.highest(rsi_val, stoch_rsi_length)
rsi_low = ta.lowest(rsi_val, stoch_rsi_length)
stoch_rsi_val = (rsi_val - rsi_low) / (rsi_high - rsi_low)

// === Conditions ===
long_condition = (close < lower_band) and (adx > adx_threshold) and (rsi_val < rsi_lower) and (stoch_rsi_val < stoch_rsi_lower)
short_condition = (close > upper_band) and (adx > adx_threshold) and (rsi_val > rsi_upper) and (stoch_rsi_val > stoch_rsi_upper)

// === Strategy Execution ===
if (long_condition)
    strategy.entry(""Long"", strategy.long)

if (short_condition)
    strategy.entry(""Short"", strategy.short)

// Risk Management
stop_loss = close - 2 * atr
take_profit = close + 4 * atr
strategy.exit(""Exit Long"", ""Long"", stop=stop_loss, limit=take_profit)
strategy.exit(""Exit Short"", ""Short"", stop=stop_loss, limit=take_profit)

// === Plot Buy/Sell Signals ===
plotshape(series=long_condition, color=color.green, style=shape.labelup, location=location.belowbar, text=""BUY"")
plotshape(series=short_condition, color=color.red, style=shape.labeldown, location=location.abovebar, text=""SELL"")";
        }
    }
}