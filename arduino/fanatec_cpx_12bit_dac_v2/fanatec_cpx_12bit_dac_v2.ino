#include <SPI.h>         // Remember this line!
#include <DAC_MCP49xx.h>

#define INPUT_BYTES 6

byte data[INPUT_BYTES];

DAC_MCP49xx dac1(DAC_MCP49xx::MCP4922, 10, -1);
DAC_MCP49xx dac2(DAC_MCP49xx::MCP4922, 9, -1);

void setup() { 
  Serial.begin(115200);

  dac1.setSPIDivider(SPI_CLOCK_DIV2);
  dac1.setPortWrite(true); // Pin 10 only!
  dac1.setAutomaticallyLatchDual(false);

  dac2.setSPIDivider(SPI_CLOCK_DIV2);
  dac2.setPortWrite(false);
  dac2.setAutomaticallyLatchDual(false);
  
  resetPins();

  checkForStartLine();
}

void checkForStartLine(){
  int ready = 0;
  while (!ready){
    while (Serial.available()){
      char c = (char)Serial.read();
      if (c == '\n'){
        ready = 1;
        break;
      }
    }
  }
}

void loop() {
  if (Serial.available() >= INPUT_BYTES){
      for (int i=0; i < INPUT_BYTES; i++){
          data[i] = (byte)Serial.read();
      }
      dac1.output2(data[1] | data[0] << 8, data[3] | data[2] << 8);
      dac2.output2(data[5] | data[4] << 8, 0);
  }
}

void resetPins() {
  dac1.output2(0, 0);
  dac2.output2(0, 0);
}

