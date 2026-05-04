package com.kilovoid.salvager;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Scanner;

public class Main {
    public static void main(String[] args) {
        String id = IdGen.idGen();
        System.out.println(id);
        Scanner scanner = new Scanner(System.in);
        System.out.println("_____NOTE_____");
        System.out.println("Insert title:");
        String title = scanner.nextLine();

        System.out.println("Insert body text:");
        StringBuilder bodyTextBuilder = new StringBuilder();
        while (true) {
            String line = scanner.nextLine();
            if (line.isEmpty()) {
                break;
            }
            bodyTextBuilder.append(line).append("\n");
        }
        String bodyText = bodyTextBuilder.toString();

        Note note = new Note(title, bodyText, id);

        String dirPath = "notes";
        try {
            Files.createDirectories(Path.of(dirPath));
            note.saveToFile(dirPath);
            System.out.println("Note saved in " + dirPath);
        } catch (IOException e) {
            System.out.println("Error saving!" + e.getMessage());
            return;
        }

        String fileName = note.getTitle().replaceAll("\\s", "_") + ".txt";
        try {
            Note loaded = Note.loadFromFile(dirPath + "/" + fileName);
            System.out.println("Loaded Note:");
            System.out.println(loaded.getTitle());
            System.out.println("\n");
            System.out.println(loaded.getContent());
        } catch (IOException e) {
            System.out.println("Loading error!" + e.getMessage());
        }
        scanner.close();
    }
}