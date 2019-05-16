#include "Telemetry.h"
#include "AccelController.h"
#include <SimbleeBLE.h>
#include "Debug.h"

using namespace Systems;

Telemetry Systems::telemetry;

Telemetry::Telemetry()
	: telemetryActive(false)
{
}

void Telemetry::begin()
{
	telemetryActive = true;

	// Init our reuseable telemetry message
	teleMessage.type = DieMessage::MessageType_Telemetry;
	for (int i = 0; i < 1; ++i)
	{
		teleMessage.data[i] = { 0,0,0,0 };
	}

	lastAccelWasSent = false;

	// Ask the acceleration controller to be notified when
	// new acceleration data comes in!
	accelController.hook([](void* token, const AccelFrame& frame) {((Telemetry*)token)->onAccelFrame(frame); }, this);
}

void Telemetry::onAccelFrame(const AccelFrame& frame)
{
	if (lastAccelWasSent)
	{
		// Store new data in frame 0
		teleMessage.data[0].x = frame.X;
		teleMessage.data[0].y = frame.Y;
		teleMessage.data[0].z = frame.Z;
		teleMessage.data[0].deltaTime = frame.Time - lastAccelFrame.Time;
		lastAccelWasSent = false;
	}
	else
	{
		// Store new data in frame 1
		teleMessage.data[1].x = frame.X;
		teleMessage.data[1].y = frame.Y;
		teleMessage.data[1].z = frame.Z;
		teleMessage.data[1].deltaTime = frame.Time - lastAccelFrame.Time;

		// Send the message
		SimbleeBLE.send(reinterpret_cast<const char*>(&teleMessage), sizeof(DieMessageAcc));
		lastAccelWasSent = true;
	}

	// Always remember the last frame, so we can extract delta time!
	lastAccelFrame = frame;
}

void Telemetry::stop()
{
	// Stop being notified!
	accelController.unHookWithParam(this);
	telemetryActive = false;
}



