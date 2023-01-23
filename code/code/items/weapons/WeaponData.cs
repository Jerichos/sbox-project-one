namespace POLYGONWARE.ProjectOne;

public interface IWeaponData
{
	// view/model paths
	string ViewModelPath { get; }
	string ModelPath { get; }
	
	// sounds
	public string SoundPath { get; }
	
	// fire rates
	float PrimaryRate { get; }
	float SecondaryRate { get; }
	
	// shoot data
	float Spread  { get; }
	float Force  { get; }
	float Damage  { get; }
	float BulletSize  { get; }
}

public struct WeaponData : IWeaponData
{
	public string ViewModelPath { get; }
	public string ModelPath { get; }
	public string SoundPath { get; }
	public float PrimaryRate { get; }
	public float SecondaryRate { get; }
	public float Spread { get; }
	public float Force { get; }
	public float Damage { get; }
	public float BulletSize { get; }
}

public struct PistolData : IWeaponData
{
	public string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public string SoundPath => "rust_pistol.shoot";
	public float PrimaryRate => 15.0f;
	public float SecondaryRate => 1.0f;
	public float Spread => 0.05f;
	public float Force => 1.5f;
	public float Damage => 9.0f;
	public float BulletSize => 3.0f;
}
