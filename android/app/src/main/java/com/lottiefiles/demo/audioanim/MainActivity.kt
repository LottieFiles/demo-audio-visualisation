package com.lottiefiles.demo.audioanim

import android.Manifest
import android.animation.ValueAnimator
import android.content.pm.PackageManager
import android.media.AudioFormat
import android.media.AudioRecord
import android.media.MediaRecorder
import android.os.Bundle
import android.os.Handler
import android.view.animation.AccelerateInterpolator
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import com.lottiefiles.demo.audioanim.databinding.ActivityMainBinding


class MainActivity : AppCompatActivity() {

    // stuff
    private val PERMISSION_REQUEST = 555
    private lateinit var binding: ActivityMainBinding


    private val SAMPLING_INTERVAL = 80L                // how frequently do we sample the audio
    private var recorder: AudioRecord? = null
    private lateinit var animator: ValueAnimator       // Custom animator to drive the lottie

    private val am = AudioMeter(                       // initialize audio meter with some defaults
            alpha = 0.7,
            preGain = 0.5,
            min_dB = 45.0,
            max_dB = 80.0
    )


    private val minBuffer = AudioRecord.getMinBufferSize(   // minimum audio buffer as reported by the system
        16000,
        AudioFormat.CHANNEL_IN_MONO,
        AudioFormat.ENCODING_PCM_16BIT
    )

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        // configure the lottie view
        with(binding.lottieView) {
            setMinAndMaxFrame(0,33)  // the relevant animation limit lies here
//            setMinAndMaxFrame(48,78)  // the relevant animation limit lies here


            setOnClickListener {
                when(recorder?.recordingState) {
                    AudioRecord.RECORDSTATE_RECORDING -> {
                        recorder?.stop()
                        binding.textPrompt.setText(R.string.prompt)
                    }
                    else -> checkPermsAndListen()
                }
            } //end onclick
        }


        // init animator
        animator = with(ValueAnimator.ofFloat(0.0f)) {
            interpolator = AccelerateInterpolator()     // tip: play with this for effect
            duration = SAMPLING_INTERVAL

            // bind it to the lottie
            addUpdateListener {
                val progress = it.animatedValue as Float
                binding.lottieView.progress = progress
            }

            this
        }


        // sliders
        with(binding.sliderPregain) {

            value = am.preGain.toFloat()

            addOnChangeListener { _, _, _ ->
                    am.preGain = value.toDouble()
            }
        }

        with(binding.sliderInputRange) {

            setValues(am.min_dB.toFloat(), am.max_dB.toFloat())
            addOnChangeListener { _, _, _ ->
                am.min_dB = values[0].toDouble()
                am.max_dB = values[1].toDouble()
            }
        }

    }


    /**
     * convenience extension on value animator.
     * stops it in its tracks and redirects to a new end stop
     */
    private fun ValueAnimator.reanimate(new_stop: Double){
        cancel()
        val currentStop =animatedValue as Float
        val maxStop = new_stop.coerceAtMost(1.0).toFloat()
        setFloatValues(currentStop, maxStop)
        start()
    }


    private fun sampleAudio(){
        if(recorder?.recordingState == AudioRecord.RECORDSTATE_RECORDING) {

            // read audio buffer from the recorder
            val buffer = ShortArray(minBuffer)
            recorder?.read(buffer, 0, minBuffer)

            val peak = am.getLevel(buffer)
            val progress = am.getProgress(peak)

            // update animation
            animator.reanimate(progress)

            //loop for the next iteration
            Handler(mainLooper).postDelayed({ sampleAudio() }, SAMPLING_INTERVAL)

        }
    }

    private fun listen() {
        // lazy init audio recorder
        if(recorder == null) {

            recorder = AudioRecord(
                MediaRecorder.AudioSource.MIC,
                16000,
                AudioFormat.CHANNEL_IN_MONO,
                AudioFormat.ENCODING_PCM_16BIT,
                minBuffer
            )

        }

        // start recording and sampling
        recorder?.startRecording()
        sampleAudio()

        // ui update
        binding.textPrompt.setText(R.string.prompt_stop)

    }


    private fun checkPermsAndListen() {
        if( ActivityCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED ) {
            listen()
        } else {
            ActivityCompat.requestPermissions(
                    this,
                    arrayOf(Manifest.permission.RECORD_AUDIO),
                    PERMISSION_REQUEST
            )
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        if(requestCode == PERMISSION_REQUEST) {
            if(grantResults.getOrNull(0) == PackageManager.PERMISSION_GRANTED) {
                listen()
            } else {
                Toast.makeText(this, R.string.permission_denied, Toast.LENGTH_LONG).show()
            }
        } else {
            super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        }
    }
}