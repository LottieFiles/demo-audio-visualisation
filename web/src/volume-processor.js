class VolumeProcessor extends AudioWorkletProcessor {
  constructor() {
    super();

    this.volume = 0;
  }

  process(inputs, outputs, parameters) {
    const channels = inputs[0];

    if (channels.length > 0) {
      const samples = channels[0];
      let sum = 0;

      // Calculated the squared-sum.
      for (let i = 0; i < samples.length; i++) {
        sum += samples[i] * samples[i];
      }

      // Calculate the RMS level and update the volume.
      const rms = Math.sqrt(sum / samples.length);
      this.volume = Math.max(
        rms,
        this.volume * VolumeProcessor.SMOOTHING_FACTOR
      );

      // Update volume property with the main thread.
      this.port.postMessage({ volume: this.volume });
    }

    return true;
  }
}

VolumeProcessor.SMOOTHING_FACTOR = 0.8;

registerProcessor("volume-processor", VolumeProcessor);
