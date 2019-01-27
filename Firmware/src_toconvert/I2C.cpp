// 
// Wire library wrapper for the Dice
// 

#include <Wire.h> 
#include "I2C.h"

using namespace Systems;

I2C Systems::wire;

#define SCLpin 21
#define SDApin 24

void I2C::begin()
{
	Wire.speed = 400;
	Wire.beginOnPins(SCLpin, SDApin);
}

void I2C::beginTransmission(uint8_t a)
{
	Wire.beginTransmission(a);
}

void I2C::beginTransmission(int a)
{
	Wire.beginTransmission(a);
}

void I2C::end()
{
	Wire.end();
}

uint8_t I2C::endTransmission(void)
{
	return Wire.endTransmission();
}

uint8_t I2C::endTransmission(uint8_t a)
{
	return Wire.endTransmission(a);
}

uint8_t I2C::requestFrom(uint8_t a, uint8_t b)
{
	return Wire.requestFrom(a, b);
}

uint8_t I2C::requestFrom(uint8_t a, uint8_t b, uint8_t c)
{
	return Wire.requestFrom(a, b, c);
}

uint8_t I2C::requestFrom(int a, int b)
{
	return Wire.requestFrom(a, b);
}

uint8_t I2C::requestFrom(int a, int b, int c)
{
	return Wire.requestFrom(a, b, c);
}

size_t I2C::write(uint8_t a)
{
	return Wire.write(a);
}

size_t I2C::write(const uint8_t * pa, size_t b)
{
	return Wire.write(pa, b);
}

int I2C::available(void)
{
	return Wire.available();
}

int I2C::read(void)
{
	return Wire.read();
}

int I2C::peek(void)
{
	return Wire.peek();
}

void I2C::flush(void)
{
	Wire.flush();
}

void I2C::onReceive(void(*func)(int))
{
	Wire.onReceive(func);
}

void I2C::onRequest(void(*func)(void))
{
	Wire.onRequest(func);
}

void I2C::onRequestService(void)
{
	Wire.onRequestService();
}

void I2C::onReceiveService(uint8_t* a, int b)
{
	Wire.onReceiveService(a, b);
}

