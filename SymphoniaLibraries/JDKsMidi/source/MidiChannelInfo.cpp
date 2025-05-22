#include "MidiChannelInfo.h"

MidiChannelInfo::MidiChannelInfo()
	: m_midiEvents()
	, m_channelName("")
{

}

MidiChannelInfo::~MidiChannelInfo()
{
	for (MidiEvent* midiEvent : m_midiEvents)
	{
		delete midiEvent;
	}

	m_midiEvents.clear();
}

void MidiChannelInfo::AddMidiEvent(MidiEvent* midiEvent)
{
	m_midiEvents.push_back(midiEvent);
}

void MidiChannelInfo::SetChannelName(std::string name)
{
	m_channelName = name;
}

size_t MidiChannelInfo::GetMidiEventsCount() const
{
	return m_midiEvents.size();
}

const char* MidiChannelInfo::GetChannelName() const
{
	return m_channelName.c_str();
}

MidiEvent* MidiChannelInfo::GetMidiEvent(unsigned int eventID) const
{
	size_t count = GetMidiEventsCount();
	if (eventID >= count)
	{
		return nullptr;
	}

	return m_midiEvents[eventID];
}

