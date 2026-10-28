const int led1 = 12;  // Pin for first LED
const int led2 = 13; // Pin for second LED
bool running = false; // Flag to check if blinking should happen
int potPin = A0; // Analogue pin to which the potentiometer is connected
int potValue; // Potentiometer value


void setup() {
  pinMode(led1, OUTPUT);
  pinMode(led2, OUTPUT);
  Serial.begin(9600); // Start serial communication
  delay(2000);              // Wait until USB is stable
  Serial.println("READY");  // Notify Unity that Arduino is ready
}

void loop() {
  if (Serial.available() > 0) {

        char command = (char)Serial.read();
    if (command == 'A') {
      running = true;
    } else if (command == 'B') {
      running = false;
      digitalWrite(led1, LOW);
      digitalWrite(led2, LOW);
    }
  }

  if (running) {
    static unsigned long lastToggleTime = 0;
    static bool ledState = false;
    
    if (millis() - lastToggleTime >= 500) {
      lastToggleTime = millis();
      ledState = !ledState;
      digitalWrite(led1, ledState);
      digitalWrite(led2, !ledState);
    }
  }

    potValue = analogRead(potPin); // Read the potentiometer value
    Serial.println(potValue); // Send the value to the serial port
    delay(50); // Slight delay for more stable values
}
