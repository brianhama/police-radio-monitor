# police-radio-monitor
 This simple script uses OpenAI's Whsiper model to monitor a police radio channel for a given keyword.

## Background
I reside in Palo Alto, where I'm surrounded by elderly neighbors who enjoy contacting the police whenever they think my music is too loud. Given the nature of Palo Alto, the police have little else to handle, so they take these complaints very seriously.

To outsmart my neighbors and showcase the capabilities and simplicity of OpenAI's new speech transcription model, Whisper, I devised a straightforward script. For several years, I've been streaming the Palo Alto Police's dispatch radio transmissions on www.paloaltopolice.org. To accomplish this, I installed an antenna on my garage roof and employed a Uniden TrunkTracker to decode the Silicon Valley Regional Communications System's P25 Phase II radio system. Although I attempted to use SDR instead of Uniden, the latter proved to provide clearer decoding. The Uniden device connects to a basement computer, and the radio transmissions are forwarded to a service for archiving.

I noticed that every time the police address a noise complaint at my residence, a radio transmission containing my home address is dispatched so that an officer can respond. This C# script in the repository listens to the same audio stream. It establishes an audio stream using NAudio, processes the audio data with the MathNet.Numerics library, and detects radio transmissions based on a simple threshold. When the maximum amplitude of the audio signal's frequency spectrum surpasses the threshold, an in-memory audio file recording commences. The recording ceases when the amplitude returns to baseline, and the audio segment is then sent to OpenAI's Whisper API for transcoding.

After transcoding, the text is searched for a specific keyword - in this case, my home's street name. Since my street is short, this approach shouldn't produce many false positives. If the keyword is detected, an HTTP request is sent to my home automation controller (Control4), which subsequently turns off the entire-house audio system and displays a text alert on the wall screens.

I should clarify that I don't intend to use this script to actually undermine the police or annoy my neighbors. However, it serves as an excellent example of the power and simplicity of OpenAI's new Whisper model.