var target : Transform; 
private var fixedistance : float;
private var realdistance : float;
private var vardistance : float;
private var x = 0.0; 
private var y = 0.0;
private var prevx = 0.0; 
private var prevy = 0.0;
private var prevd;
private var prevf;
static var animating : boolean;

function Start () { 
    var angles = transform.eulerAngles; 
    x = angles.y; 
    y = angles.x; 
	fixedistance = 6.0;
	prevd = realdistance;
	prevf = fixedistance;
	animating = true;
    Animate();
} 
    
function LateUpdate () { 
	if ((Input.GetMouseButtonDown(0)) || (Input.GetMouseButtonDown(1))) animating = !animating;
	
   fixedistance += -(Input.GetAxis("Mouse ScrollWheel") * 0.4) * Mathf.Abs(fixedistance);
   
   if ((fixedistance != prevf) || (realdistance != prevd) || (x != prevx) || (y != prevy)) {
   		y = ClampAngle(y, -60, 60);
   		x = ClampAngle(x, -60, 60);
	    fixedistance = Mathf.Clamp(fixedistance, 2, 10);
	    var rotation = Quaternion.Euler(y, x, 0);
	    var position = rotation * Vector3(0.0, 0.0, -realdistance) + target.position;
	    transform.rotation = rotation;
	    transform.position = position;
    }
    prevx = x;
    prevy = y;
    prevd = realdistance;
    prevf = fixedistance;
} 

static function ClampAngle (angle : float, min : float, max : float) { 
   if (angle < -360) angle += 360; 
   if (angle > 360) angle -= 360; 
   return Mathf.Clamp (angle, min, max); 
}

function Animate() {
	while (true) {
		if (animating){
			x = Mathf.Lerp(x, 60 * Mathf.Cos(Time.time * 0.25), Time.deltaTime);
			y = Mathf.Lerp(y, 40 * Mathf.Sin(Time.time * 0.50), Time.deltaTime);
			vardistance = Mathf.Lerp(vardistance, (1.5 * Mathf.Cos(Time.time * 0.50)), Time.deltaTime);
			x = ClampAngle(x, -60, 60);
			y = ClampAngle(y, -60, 60);
		}
		realdistance = fixedistance + vardistance;
		yield;
	}
}