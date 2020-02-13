using KSP.Localization;

namespace KERBALISM
{


	public static class Communications
	{
		/// <summary> True if CommNet is initialized </summary>
		public static bool NetworkInitialized = false;

		/// <summary> True if CommNet initialization has begin </summary>
		public static bool NetworkInitializing = false;

		public static void Update(Vessel v, VesselData vd, double elapsed_s)
		{
			if(!Lib.IsVessel(v))
				return;

			// EC consumption is handled in Science update

			Cache.WarpCache(v).dataCapacity = vd.deviceTransmit ? vd.Connection.rate * elapsed_s * vd.ResHandler.ElectricCharge.AvailabilityFactor : 0.0;

			// do nothing if network is not ready
			if (!NetworkInitialized) return;

			// maintain and send messages
			// - do not send messages during/after solar storms
			// - do not send messages for EVA kerbals
			if (!v.isEVA && v.situation != Vessel.Situations.PRELAUNCH)
			{
				if (!vd.msg_signal && !vd.Connection.linked)
				{
					vd.msg_signal = true;
					if (vd.cfg_signal)
					{
						string subtext = Local.UI_transmissiondisabled;

						switch (vd.Connection.status)
						{

							case LinkStatus.plasma:
								subtext = Local.UI_Plasmablackout;
								break;
							case LinkStatus.storm:
								subtext = Local.UI_Stormblackout;
								break;
							default:
								if (vd.CrewCount == 0)
								{
									switch (Settings.UnlinkedControl)
									{
										case UnlinkedCtrl.none:
											subtext = Local.UI_noctrl;
											break;
										case UnlinkedCtrl.limited:
											subtext = Local.UI_limitedcontrol;
											break;
									}
								}
								break;
						}

						Message.Post(Severity.warning, Lib.BuildString(Local.UI_signallost, " <b>", v.vesselName, "</b>"), subtext);
					}
				}
				else if (vd.msg_signal && vd.Connection.linked)
				{
					vd.msg_signal = false;
					if (vd.cfg_signal)
					{
						Message.Post(Severity.relax, Lib.BuildString("<b>", v.vesselName, "</b> ", Local.UI_signalback),
						  vd.Connection.status == LinkStatus.direct_link ? Local.UI_directlink :
							Lib.BuildString(Local.UI_relayby, " <b>", vd.Connection.target_name, "</b>"));
					}
				}
			}
		}
	}


} // KERBALISM

