const int trigPin = 6;
const int echoPin = 7;



void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  
  delay(2000);
  Serial.println("READY");


}

void loop() {
  // put your main code here, to run repeatedly:
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);

  unsigned long duration = pulseIn(echoPin, HIGH, 25000);
  

  if(duration != 0)
  {
    float distancecm = duration * 0.0343 / 2.0;
    Serial.println(distancecm, 1);
  }


}
