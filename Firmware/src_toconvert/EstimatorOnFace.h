// EstimatorOnFace.h

#ifndef _ESTIMATORONFACE_h
#define _ESTIMATORONFACE_h

#include "Arduino.h"
#include "IStateEstimator.h"
#include "float3.h"
#include "AccelController.h"

class EstimatorOnFace
	: public IStateEstimator
{
private:
	Core::float3 canons[3]; // May not be exactly aligned with x y and z...
	Core::float3 avgAcc;
	float sigmaAccSq;
	int face;

private:
	static void accelControllerCallback(void* me, const AccelFrame& frame);
	void onNewFrame(const AccelFrame& frame);

public:
	EstimatorOnFace();
	void begin();
	virtual float GetEstimate() override;
};

#endif

