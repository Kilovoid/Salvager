/// Этот класс отвечает за саму заметку, каждую из них. Именно тут создается ее экземпляр. Сеттеров нет, так как пока
/// что не понадобилось.

package com.kilovoid.salvager;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

public class Note {
    private String title;
    private String content;

    //Constructor
    public Note(String title, String content) {
        this.title = title;
        this.content = content;
    }

    public String getTitle() {
        return title;
    }
    public String getContent() {
        return content;
    }

    public void saveToFile(String dirPath) throws IOException {
        String fileName = title.replaceAll("\\s+", "_") + ".txt";
        Path path = Path.of(dirPath, fileName);
        String fileContent = title + "\n\n" + content;
        Files.writeString(path, fileContent);
    }

    public static Note loadFromFile(String filePath) throws IOException {
        Path path = Path.of(filePath);
        String content = Files.readString(path);
        String[] lines = content.split("\n",3);
        if (lines.length < 2) {
            throw new IOException("Invalid file format");
        }
        String title = lines[0].trim();
        String body = lines.length > 2 ? lines[2] : "";

        return new Note(title, body);
    }
}
