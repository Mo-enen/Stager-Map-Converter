namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;


	public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {



		// VAR
		public static Text TipLabel { get; set; } = null;

		[TextArea(6, 12), SerializeField] private string m_TipKey = "";


		// MSG
		private void OnDisable () => TipLabel.text = "";
		public void OnPointerEnter (PointerEventData e) => TipLabel.text = m_TipKey;
		public void OnPointerExit (PointerEventData e) => TipLabel.text = "";


	}
}