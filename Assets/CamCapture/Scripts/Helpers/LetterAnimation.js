private var origcolor : Color;
private var myMR : MeshRenderer;
private var illum : boolean;

function Start() {
	var animating = false;
	myMR = gameObject.GetComponent(MeshRenderer);
	origcolor = myMR.materials[0].color;
}

function Update() {
	if (illum){
		myMR.materials[0].color = Color.Lerp(myMR.materials[0].color, Color.white, Time.deltaTime * 5);	
	}else{
		if(myMR.materials[0].color != origcolor){
			myMR.materials[0].color = Color.Lerp(myMR.materials[0].color, origcolor, Time.deltaTime * 5);
		}	
	}
}


function OnMouseEnter(){
	illum = true;
	Animate();
}

function OnMouseExit(){
	illum = false;
}

function Animate(){
	for(var i=0; i<180 ; i++){
		transform.Rotate(Vector3.up * 2, Space.World);
		yield;	
	}
}