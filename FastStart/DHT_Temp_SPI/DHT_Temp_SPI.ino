#include <dht.h>
#include <math.h>
#include <OneWire.h>
#include <SPI.h>
#include <Wire.h>

dht DHT;

#define DHT11_PIN 2
#define SLAVE_ADDRESS 0x04


int sensorPin = A3;
OneWire ds(3);

String wireData;

double Thermistor(int RawADC) 
{
  double Temp;
  Temp = log(10000.0*((1024.0/RawADC-1))); 
  Temp = 1 / (0.001129148 + (0.000234125 + (0.0000000876741 * Temp * Temp ))* Temp );
  Temp = Temp - 273.15;            // Convert Kelvin to Celcius
  return Temp;
}

double GetTemperature()
{
  //For conversion of raw data to C
  int HighByte, LowByte, TReading, SignBit, Tc_100;
 
  byte i;
  byte present = 0;
  byte data[12];
  byte addr[8];
 
 
  while ( !ds.search(addr))
  {
      ds.reset_search();
  }

  if ( OneWire::crc8( addr, 7) != addr[7])
  {
      return -999.1;
  }
 
  if ( addr[0] == 0x10) {
      //Device is a DS18S20 family device.
  }
  else if ( addr[0] == 0x28) {
      //Device is a DS18B20 family device.
  }
  else {
      //Device family is not recognized: 0x
      return -999.2;
  }
 
  ds.reset();
  ds.select(addr);
  ds.write(0x44,1);         // start conversion, with parasite power on at the end
 
  delay(1000);     // maybe 750ms is enough, maybe not
  // we might do a ds.depower() here, but the reset will take care of it.
 
  present = ds.reset();
  ds.select(addr);    
  ds.write(0xBE);         // Read Scratchpad
 
  for ( i = 0; i < 9; i++) {           // we need 9 bytes
    data[i] = ds.read();
  }
 
  //Conversion of raw data to C
  LowByte = data[0];
  HighByte = data[1];
  TReading = (HighByte << 8) + LowByte;
  SignBit = TReading & 0x8000;  // test most sig bit
  if (SignBit) // negative
  {
    TReading = (TReading ^ 0xffff) + 1; // 2's comp
  }
  Tc_100 = (6 * TReading) + TReading / 4;    // multiply by (100 * 0.0625) or 6.25
 
  double temp = ((double)Tc_100/100);
  if (SignBit) // If its negative
  {
     temp = 0 - temp;
  }
  return temp;
}

void setup()
{
  Serial.begin(115200);
  
  // have to send on master in, *slave out*
  pinMode(MISO, OUTPUT);

  // turn on SPI in slave mode
  SPCR |= _BV(SPE);
  
  //I2C setup
  pinMode(13, OUTPUT);

  // initialize i2c as slave
  Wire.begin(SLAVE_ADDRESS);

  // define callbacks for i2c communication
  //Wire.onReceive(receiveData);
  Wire.onRequest(sendData);




  Serial.println("Hum(%)\tT1(C)\tT2(C)\tT3(C)");
}

void sendData()
{
  char dataArray[wireData.length()];
  wireData.toCharArray(dataArray, wireData.length());
  for (int i=0; i < sizeof(dataArray); i++)
  {
    Wire.write(dataArray[i]);
  }
}

void loop()
{
  // READ DATA
  //DHT.read11(DHT11_PIN);
  
  //String output = String(DHT.humidity) + "\t" + DHT.temperature + "\t";
  
  // DISPLAY DATA
  //Serial.print(DHT.humidity, 1);
  //Serial.print("\t");
  //Serial.print(DHT.temperature, 1);
  
  int readVal=analogRead(sensorPin);
  double temp =  Thermistor(readVal);
  //output = output + String(temp) + "\t";
  String output = String(temp) + "\t" + String(temp) + "\t" + String(temp) + "\t" + String(temp) + ";\t";
  //Serial.print("\t");
  //Serial.print(temp, 2);

  //temp = GetTemperature();
  //output = output + String(temp) + ";\t";
  //Serial.print("\t");
  Serial.println(temp, 2);


  char dataArray[output.length()];
  output.toCharArray(dataArray, output.length());
  wireData = output;
  
  for (int i=0; i < output.length(); i++)
  {
    SPI.transfer(dataArray[i]);   // Send 8 bits
  }
 
  //delay(500);
  delay(100);
}
//
// END OF FILE
//


