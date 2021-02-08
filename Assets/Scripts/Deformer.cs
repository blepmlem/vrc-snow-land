using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class Deformer : UdonSharpBehaviour
{
	[SerializeField]
	private bool _isPlayerTracker;

	[SerializeField]
	private ParticleSystem _particles;

	[SerializeField]
	private float _hitRadius = .7f;

	[SerializeField]
	private float _minMoveSqrMagnitude = 1;

	[SerializeField]
	private AudioSource[] _audioSources;

	private ParticleSystem.EmissionModule _emission;
	private VRCPlayerApi _player;
	private Transform _target;
	private int _layer;
	private float _timeSliceInterval = .1f;
	private float _timeSliceTimer;
	private Vector3 _lastPosition;
	private int _lastAudioIndex;
	private int _audioSourcesLength;
	private void Start()
	{
		_audioSourcesLength = _audioSources.Length;
		_emission = _particles.emission;
		_emission.enabled = false;
		_layer = 1 << 23; //"Deformable"
		if (!_isPlayerTracker)
		{
			var t = transform;
			_target = t.parent;
			t.parent = null;
			t.rotation = Quaternion.identity;
		}
	}

	private void Update()
	{
		_timeSliceTimer += Time.deltaTime;
		if (_timeSliceTimer < _timeSliceInterval)
		{
			return;
		}

		_timeSliceTimer = 0;
		
		var t = transform;
		_lastPosition = t.position;
		
		if (!_isPlayerTracker)
		{
			t.position = _target.position;
		}
		else if (_player != null)
		{
			t.position = _player.GetPosition();
		}
		else
		{
			enabled = false;
			return;
		}

		bool hit = Physics.CheckSphere(t.position, _hitRadius, _layer);
		_emission.enabled = hit;
		
		if (hit && (t.position - _lastPosition).sqrMagnitude > _minMoveSqrMagnitude)
		{
			AudioSource audioSource;
			if(_lastAudioIndex < _audioSourcesLength)
			{
				int rand = Random.Range(0, _audioSourcesLength-1);
				if(rand >= _lastAudioIndex)
				{
					rand++;
				}

				_lastAudioIndex = rand;
				audioSource = _audioSources[rand];
			}
			else
			{
				_lastAudioIndex = Random.Range(0, _audioSourcesLength);
				audioSource = _audioSources[Random.Range(0, _audioSourcesLength)];
			}
			if(!audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}
	}

	public void SetPlayer(VRCPlayerApi player)
	{
		_player = player;
		enabled = true;
		Debug.Log($"{gameObject.name} has been assigned to {player.displayName}");
	}

	public void RemovePlayer()
	{
		_player = null;
		enabled = false;
	}
}