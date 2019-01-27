#include "EstimatorOnFace.h"
#include "AccelController.h"

using namespace Core;

#define CANON_THRESHOLD 0.98f

EstimatorOnFace::EstimatorOnFace()
{
}

void EstimatorOnFace::accelControllerCallback(void* me, const AccelFrame& frame)
{
	static_cast<EstimatorOnFace*>(me)->onNewFrame(frame);
}

void EstimatorOnFace::onNewFrame(const AccelFrame& frame)
{
	// Fetch the values from the last 500ms and compute an average direction and variance
	auto& buffer = accelController.getBuffer();

	// Average acceleration first
	avgAcc = float3::zero();
	for (int i = buffer.count() / 2; i < buffer.count(); ++i)
	{
		auto& frame = buffer[i];
		avgAcc += frame.getAcc();
	}
	avgAcc /= buffer.count() / 2;

	// Variance (or standard deviation squared)
	sigmaAccSq = 0.0f;
	for (int i = buffer.count() / 2; i < buffer.count(); ++i)
	{
		auto& frame = buffer[i];
		sigmaAccSq += (frame.getAcc() - avgAcc).sqrMagnitude();
	}
	sigmaAccSq /= buffer.count() / 2 - 1; // Variance is over N-1 when there are N samples

	// Which canonical orientation is the acc closest to?
	float xmag = float3::dot(avgAcc, canons[0]);
	float ymag = float3::dot(avgAcc, canons[1]);
	float zmag = float3::dot(avgAcc, canons[2]);
	//if (xmag > ymag)
	//{
	//	if (xmag > zmag)
	//	{
	//		// It's positive X, is it really close?
	//		if (xmag > CANON_THRESHOLD)
	//		{
	//			face = 4;
	//		}
	//		else
	//		{
	//			face = -1;
	//		}
	//	}
	//	else
	//	{
	//		// positive Z
	//	}
	//}
}

void EstimatorOnFace::begin()
{
	// Initialize our canonical directions
	// Ultimately we'll calibrate these values and store in flash somewhere
	canons[0] = float3(1.0f, 0.0f, 0.0f);
	canons[1] = float3(0.0f, 1.0f, 0.0f);
	canons[2] = float3(0.0f, 0.0f, 1.0f);

	avgAcc = float3::zero();
	sigmaAccSq = 0.0f;

	accelController.hook(accelControllerCallback, this);
}

float EstimatorOnFace::GetEstimate()
{
	// We are pretty confident the die is on one of its faces iff
	// - The acceleration hasn't changed much in the last 500ms
	// - It is aligned with one of the cardinal orientations (or 180 degrees from it)
	return 0.0f;
}

