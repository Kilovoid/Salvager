/// Этот класс отвечает за саму заметку, каждую из них. Именно тут создается ее экземпляр. Сеттеров нет, так как пока
/// что не понадобилось.

package com.kilovoid.salvager;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

public class Note {
    private String title;
    private String content;
    private String id;

    //Constructor
    public Note(String title, String content, String id) {
        this.title = title;
        this.content = content;
        this.id = id;
    }

    public String getTitle() {
        return title;
    }
    public String getContent() {
        return content;
    }
    public String getId() { return id; }

    public void setId(String id) { this.id = id; }
}
