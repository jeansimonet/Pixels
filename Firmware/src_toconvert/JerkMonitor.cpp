#include "JerkMonitor.h"
#include "Accelerometer.h"
#include "Lazarus.h"
#include "Debug.h"

using namespace Systems;
using namespace Devices;

JerkMonitor Systems::jerkMonitor;

#define SIGMA_DECAY (0.95f)
#define SIGMA_THRESHOLD (20)

void JerkMonitor::begin()
{
	sigma = 0.0f;
	accelController.hook(accelControllerCallback, nullptr);
}

void JerkMonitor::accelControllerCallback(void* ignore, const AccelFrame& frame)
{
	jerkMonitor.onAccelFrame(frame);
}

void JerkMonitor::onAccelFrame(const AccelFrame& frame)
{
	// Add the last frame
	float jerkX = accelerometer.convert(frame.jerkX);
	float jerkY = accelerometer.convert(frame.jerkY);
	float jerkZ = accelerometer.convert(frame.jerkZ);
	float jerk2 = jerkX * jerkX + jerkY * jerkY + jerkZ * jerkZ;
	sigma = sigma * SIGMA_DECAY + jerk2 * (1.0f - SIGMA_DECAY);
	//debugPrint("Sigma: ");
	//debugPrintln(sigma);

	// If the current sigma is high enough (i.e. the die is moving) poke lazarus
	if (sigma > SIGMA_THRESHOLD)
	{
		lazarus.poke();
	}
}



