#pragma once

#include "../utils/BulkDataTransfer.h"

namespace Services
{
    namespace Settings
    {
        void init();

		/// <summary>
		/// This defines a state machine that can manage receiving the dice settings over bluetooth
		/// and then update them in flash
		/// </summary>
		class ReceiveSettingsSM
		{
		private:
			enum State
			{
				State_ErasingFlash = 0,
				State_SendingAck,
				State_TransferSettings,
				State_Failed,
				State_Done
			};

			State currentState;
			ReceiveBulkDataSM receiveBulkDataSM;

			typedef void(*FinishedCallback)(void* token);
			FinishedCallback FinishedCallbackHandler;
			void* FinishedCallbackToken;

		private:
			void Finish();

		public:
			ReceiveSettingsSM();
			void Setup(void* token, FinishedCallback handler);
			void Update();
		};

		/// <summary>
		/// This defines a state machine that can send the current settings over
		/// bluetooth to the phone. Typically so the phone can edit it and redownload it.
		/// </summary>
		class SendSettingsSM
		{
		private:
			enum State
			{
				State_SendingSetup,
				State_WaitingForSetupAck,
				State_SetupAckReceived,
				State_SendingSettings,
				State_Failed,
				State_Done
			};

			State currentState;

			// Temporarily stores animation pointers as we program them in flash
			SendBulkDataSM sendBulkDataSM;

			typedef void(*FinishedCallback)(void* token);
			FinishedCallback FinishedCallbackHandler;
			void* FinishedCallbackToken;

		private:
			void Finish();

		public:
			SendSettingsSM();
			void Setup(void* token, FinishedCallback handler);
			void Update();
		};
    }
}

