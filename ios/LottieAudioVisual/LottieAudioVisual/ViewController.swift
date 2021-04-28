//
//  ViewController.swift
//  LottieAudioVisual
//
//  Created by Evandro Harrison Hoffmann on 28/04/2021.
//

import UIKit
import Lottie
import AVFoundation

class ViewController: UIViewController {
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view.
        
        setupAnimation()
        setupRecorder()
    }
    
    // MARK: - Animation
    
    @IBOutlet weak var animationView: AnimationView!
    
    private func setupAnimation() {
        animationView.contentMode = .scaleAspectFit
        Animation.loadedFrom(url: URL(string: "https://assets1.lottiefiles.com/datafiles/QeC7XD39x4C1CIj/data.json")!, closure: { [animationView] animation in
            animationView?.animation = animation
        }, animationCache: LRUAnimationCache.sharedCache)
    }
    
    // MARK: - Audio Recorder
    
    private var audioRecorder: AVAudioRecorder?
    private var timer: Timer?
    private var audioSample: Float = 0
    
    private func setupRecorder() {
        let audioSession = AVAudioSession.sharedInstance()
        if audioSession.recordPermission != .granted {
            audioSession.requestRecordPermission { (isGranted) in
                if !isGranted {
                    fatalError("You must allow audio recording for this demo to work")
                }
            }
        }
        
        let url = URL(fileURLWithPath: "/dev/null", isDirectory: true)
        let recorderSettings: [String:Any] = [
            AVFormatIDKey: NSNumber(value: kAudioFormatAppleLossless),
            AVSampleRateKey: 44100.0,
            AVNumberOfChannelsKey: 1,
            AVEncoderAudioQualityKey: AVAudioQuality.min.rawValue
        ]
        
        do {
            audioRecorder = try AVAudioRecorder(url: url, settings: recorderSettings)
            try audioSession.setCategory(.playAndRecord, mode: .default, options: [])
            
            startMonitoring()
        } catch {
            fatalError(error.localizedDescription)
        }
    }
    
    private func startMonitoring() {
        audioRecorder?.isMeteringEnabled = true
        audioRecorder?.record()
        timer = Timer.scheduledTimer(withTimeInterval: 0.01, repeats: true, block: { [audioRecorder, weak self] (timer) in
            audioRecorder?.updateMeters()
            self?.updateAudioMeter(audioRecorder?.averagePower(forChannel: 0) ?? 0)
        })
    }
    
    private func updateAudioMeter(_ power: Float) {
        let level = max(0.2, CGFloat(power) + 50) / 2
        let progress = level/25
        
        animationView.currentFrame = 33*progress
    }
    
    deinit {
        timer?.invalidate()
        audioRecorder?.stop()
    }
    
}
