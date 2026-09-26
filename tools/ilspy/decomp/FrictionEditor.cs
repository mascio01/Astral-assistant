using System.Collections.Generic;

public class FrictionEditor : DebugMenu
{
	public FrictionEditor()
		: base("Friction Editor")
	{
		Items.Add(new DebugMenuFloatAdjuster("Forward Extremum Slip", 0f, 2f, () => VehicleBehaviour.ForwardFrictionExtremumSlip, delegate(float v)
		{
			VehicleBehaviour.ForwardFrictionExtremumSlip = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Forward Extremum Value", 0f, 10f, () => VehicleBehaviour.ForwardFrictionExtremumValue, delegate(float v)
		{
			VehicleBehaviour.ForwardFrictionExtremumValue = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Forward Asymptote Slip", 0f, 5f, () => VehicleBehaviour.ForwardFrictionAsymptoteSlip, delegate(float v)
		{
			VehicleBehaviour.ForwardFrictionAsymptoteSlip = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Forward Asymptote Value", 0f, 5f, () => VehicleBehaviour.ForwardFrictionAsymptoteValue, delegate(float v)
		{
			VehicleBehaviour.ForwardFrictionAsymptoteValue = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Forward Stiffness", 0f, 2f, () => VehicleBehaviour.ForwardFrictionStiffness, delegate(float v)
		{
			VehicleBehaviour.ForwardFrictionStiffness = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Sideways Extremum Slip", 0f, 2f, () => VehicleBehaviour.SidewaysFrictionExtremumSlip, delegate(float v)
		{
			VehicleBehaviour.SidewaysFrictionExtremumSlip = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Sideways Extremum Value", 0f, 10f, () => VehicleBehaviour.SidewaysFrictionExtremumValue, delegate(float v)
		{
			VehicleBehaviour.SidewaysFrictionExtremumValue = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Sideways Asymptote Slip", 0f, 5f, () => VehicleBehaviour.SidewaysFrictionAsymptoteSlip, delegate(float v)
		{
			VehicleBehaviour.SidewaysFrictionAsymptoteSlip = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Sideways Asymptote Value", 0f, 5f, () => VehicleBehaviour.SidewaysFrictionAsymptoteValue, delegate(float v)
		{
			VehicleBehaviour.SidewaysFrictionAsymptoteValue = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		Items.Add(new DebugMenuFloatAdjuster("Sideways Stiffness", 0f, 2f, () => VehicleBehaviour.SidewaysFrictionStiffness, delegate(float v)
		{
			VehicleBehaviour.SidewaysFrictionStiffness = v;
			VehicleBehaviour.OnFrictionSettingsChanged();
		}));
		for (int num = 0; num < 3; num++)
		{
			int localIndex = num;
			List<DebugMenuItem> items = Items;
			GroundType groundType = (GroundType)num;
			items.Add(new DebugMenuFloatAdjuster(groundType.ToString() + " Extremum", 0f, 2f, () => VehicleBehaviour.TerrainExtremumFactor[localIndex], delegate(float v)
			{
				VehicleBehaviour.TerrainExtremumFactor[localIndex] = v;
				VehicleBehaviour.OnFrictionSettingsChanged();
			}));
			List<DebugMenuItem> items2 = Items;
			groundType = (GroundType)num;
			items2.Add(new DebugMenuFloatAdjuster(groundType.ToString() + " Extremum In Snow", 0f, 2f, () => VehicleBehaviour.TerrainExtremumFactorInSnow[localIndex], delegate(float v)
			{
				VehicleBehaviour.TerrainExtremumFactorInSnow[localIndex] = v;
				VehicleBehaviour.OnFrictionSettingsChanged();
			}));
			List<DebugMenuItem> items3 = Items;
			groundType = (GroundType)num;
			items3.Add(new DebugMenuFloatAdjuster(groundType.ToString() + " Asymptote", 0f, 2f, () => VehicleBehaviour.TerrainAsymptoteFactor[localIndex], delegate(float v)
			{
				VehicleBehaviour.TerrainAsymptoteFactor[localIndex] = v;
				VehicleBehaviour.OnFrictionSettingsChanged();
			}));
			List<DebugMenuItem> items4 = Items;
			groundType = (GroundType)num;
			items4.Add(new DebugMenuFloatAdjuster(groundType.ToString() + " Asymptote In Snow", 0f, 2f, () => VehicleBehaviour.TerrainAsymptoteFactorInSnow[localIndex], delegate(float v)
			{
				VehicleBehaviour.TerrainAsymptoteFactorInSnow[localIndex] = v;
				VehicleBehaviour.OnFrictionSettingsChanged();
			}));
		}
	}
}
