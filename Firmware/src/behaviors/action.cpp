#include "action.h"
#include "data_set/data_set.h"
#include "modules/anim_controller.h"
#include "nrf_log.h"

using namespace Modules;

namespace Behaviors
{
    void triggerAction(int actionIndex) {
        // Fetch the action from the dataset
        auto action = DataSet::getAction(actionIndex);
        switch (action->type) {
            case Action_PlayAnimation:
                {
                    auto playAnimAction = static_cast<const ActionPlayAnimation*>(action);
                    if (playAnimAction->faceIndex == FACE_INDEX_CURRENT_FACE) {
                        AnimController::play(playAnimAction->animIndex, Accelerometer::currentFace(), false); // FIXME, handle remapFace and loopCount properly
                    } else {
                        AnimController::play(playAnimAction->animIndex, playAnimAction->faceIndex, false); // FIXME, handle remapFace and loopCount properly
                    }
                }
                break;
            default:
                NRF_LOG_ERROR("Unknown action type %d for action index %d", action->type, actionIndex);
                break;
        }
    }
}
