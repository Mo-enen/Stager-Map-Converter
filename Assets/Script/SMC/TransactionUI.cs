namespace StagerStudio.UI {
	using UnityEngine;
	using UnityEngine.UI;
	public class TransactionUI : MonoBehaviour {
		[SerializeField] private Image m_Background = null;
		private float AnimationTime = 0f;
		private Color BasicColor = Color.white;
		private void Awake () {
			m_Background.enabled = true;
			BasicColor = m_Background.color;
		}
		void Update () {
			const float DURATION = 1f;
			m_Background.color = Color.Lerp(BasicColor, Color.clear, AnimationTime / DURATION);
			AnimationTime += Mathf.Min(Time.deltaTime, 0.01f);
			if (AnimationTime > DURATION) {
				Destroy(gameObject);
			}
		}
	}
}