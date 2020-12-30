# Script used to provide an interface to Vokaturi to be used by .NET
# Initialises a class that loads the library and provides a method to 
# Read the emotionality of a sound array
# Manuel Hernandez Najera-Aleson (https://github.com/manherna)
import sys
import Vokaturi
import ctypes

class vokaNetWrapper:

	def __init__(self, DLLstring):
		Vokaturi.load(DLLstring)
		self.voice = []

	def vokalculate(self, soundArr, samplerate):
		'''
			Calculates the emotionality of a sound sample
		
			:param double [] soundArr: Array containing the sound
			:param int samplerate: samplerate of the sound to process

			:return dictionary containing each emotions probability from [0,1] 
							, a string with a log and a boolean (true if calculations worked)
		'''
	
		error = "Starting"
		buffer_size = len(soundArr)


		c_buffer = Vokaturi.SampleArrayC(buffer_size)
		c_buffer [:] = soundArr[:]
		
		self.voice = Vokaturi.Voice (samplerate, buffer_size)
		self.voice.fill(buffer_size, c_buffer)

		quality = Vokaturi.Quality()
		emotionProbabilities = Vokaturi.EmotionProbabilities()
		self.voice.extract(quality, emotionProbabilities)

		success = bool(quality.valid)

		if(quality.valid):
			error = error + "\n SUCCESS!"
		else:
			error = error + "\n Not enough sonorancy to determine emotions" 

		return {
			"Neutral": emotionProbabilities.neutrality,
			"Happy": emotionProbabilities.happiness,
			"Sad": emotionProbabilities.sadness,
			"Angry": emotionProbabilities.anger,
			"Fear": emotionProbabilities.fear,
			"Error": error,
			"Success" : success
		}

