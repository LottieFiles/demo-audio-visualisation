package com.lottiefiles.demo.audioanim

import kotlin.math.abs
import kotlin.math.log10
import kotlin.math.pow
import kotlin.math.sqrt

class AudioMeter(
        var alpha: Double = 0.9,     // alpha value for exponential averaging,
        var preGain: Double = 0.4,   // value for audio pre-gain
        var min_dB: Double = 45.0,   // min audio level
        var max_dB: Double = 80.0    // max audio level
) {


    private var ema = 0.0           // holds last peak average

    private fun ShortArray.RMS(): Double =
        this.map { abs(it.toDouble()).pow(2) }      // square
            .average()                                 // mean square
            .let { sqrt(it) }                          // root mean square


    private fun ShortArray.PEAK(): Double =
        this.map { abs(it.toDouble()) }   // to double
            .maxOf { it }                 // select peak


    /**
     * Calculates the level for an audio buffer ( array of short representing PCM_16 audio)
     * if usePeak = true, will use the peak amplitude
     * instead of RMS.
     */
    fun getLevel(buffer: ShortArray, usePeak:Boolean=false): Double {

        // calculate the power-level either from RMS or Peak Amblitude
        val level = if (usePeak) buffer.PEAK() else buffer.RMS()

        // apply smoothing on the level
        ema = ema * alpha + (1 - alpha) * level

        // pre-gain, convert to decibels and return
        // note: we don't pre-gain the last ema, just the output
        val preGained = preGain * ema
        return 20.0 * log10(preGained)

    }

    /**
     * calculates the input dB ratio within the cut-off range
     */
    fun getProgress(input_dB: Double): Double {
        var perc = input_dB.coerceAtLeast(min_dB) - min_dB      // lowest should be 0.0
        perc /= (max_dB - min_dB)                               //as a percentage of the range
        return perc
    }
}