package com.kilovoid.salvager;

import java.util.Random;

public class IdGen {
    private static final String ALPHA = "abcdefghijklmnopqrstuvwxyz";
    private static final Random RANDOM = new Random();
    //тут будет еще что-то про дату и время, пока генерирую рандомный ID длины 10

    public static String idGen() {
        StringBuilder charPart = new StringBuilder();
        for (int i = 0; i < 10; i++) {
            charPart.append(ALPHA.charAt(RANDOM.nextInt(ALPHA.length())));
        }
        return charPart.toString();
    }
}
