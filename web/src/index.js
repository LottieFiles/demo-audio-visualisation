let volume = 0;
const player = document.querySelector("lottie-player");

function onMicrophoneDenied() {
  console.log("denied");
}

async function onMicrophoneGranted(stream) {
  const audioContext = new window.AudioContext();

  console.log("Setting up volume processor");

  await audioContext.audioWorklet.addModule(
    new URL("./volume-processor.js", import.meta.url)
  );

  const mediaStreamSource = audioContext.createMediaStreamSource(stream);

  const node = new AudioWorkletNode(audioContext, "volume-processor");

  node.port.onmessage = (event) => {
    if (event.data.volume) {
      volume = Math.ceil(event.data.volume * 100);
    }
  };

  mediaStreamSource.connect(node).connect(audioContext.destination);

  render();
}

function render() {
  player.seek(`${volume}%`);
  // console.log(`${volume * 1000}`)

  requestAnimationFrame(render);
}

navigator.getUserMedia(
  { audio: true },
  onMicrophoneGranted,
  onMicrophoneDenied
);
