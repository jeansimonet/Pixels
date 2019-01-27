#pragma once
#ifndef __ISTATE_ESTIMATOR__
#define __ISTATE_ESTIMATOR__

enum DieState
{
	DieState_Unknown = 0,
	DieState_Face1 = 1,
	DieState_Face2,
	DieState_Face3,
	DieState_Face4,
	DieState_Face5,
	DieState_Face6,
	DieState_Handling,
	DieState_Falling,
	DieState_Rolling,
	DieState_Jerking,
	DieState_Crooked,

	DieState_Count
};

struct StateEstimate
{
	float estimates[DieState_Count];
};

class IStateEstimator
{
public:
	virtual float GetEstimate() = 0;
};



#endif
