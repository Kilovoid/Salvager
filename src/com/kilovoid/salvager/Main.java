package com.kilovoid.salvager;

import java.io.IOException;
import java.nio.file.Path;
import java.util.List;
import java.util.Scanner;

public class Main {
    public static void main(String[] args) throws IOException {
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

        Note note = new Note(title, bodyText, null);

        NoteRepo repo = new NoteRepo("D:\\Java Projects\\Salvager");
        try {
            repo.saveNote(note);
            System.out.println("Note saved in " + repo);
        } catch (IOException e) {
            System.out.println("Error saving!" + e.getMessage());
            return;
        }
        System.out.println(note.getId());

        repo.findById("dmbtgtqlxq");
        List<Path> listOfAllFiles = repo.getAllFiles();
        System.out.println(listOfAllFiles);
        scanner.close();
    }
}