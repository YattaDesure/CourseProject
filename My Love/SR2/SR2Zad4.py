import tkinter as tk
from tkinter import messagebox, ttk


def main():
    root = tk.Tk()
    root.title("СР №2 — Задание 4 (плетёнка NxN)")
    root.geometry("760x640")

    ttk.Label(
        root,
        text="Задание 4.\nПлетёнка NxN полосок.",
        justify="left",
    ).pack(anchor="w", padx=10, pady=(10, 6))

    row = ttk.Frame(root)
    row.pack(fill="x", padx=10, pady=6)
    ttk.Label(row, text="N:").pack(side="left")
    ent_n = ttk.Entry(row, width=8)
    ent_n.pack(side="left", padx=4)
    ent_n.insert(0, "6")

    color = tk.StringVar(value="brown")  # radiobutton
    opts = ttk.LabelFrame(root, text="Цвет полосок")
    opts.pack(fill="x", padx=10, pady=6)
    ttk.Radiobutton(opts, text="Коричневый/жёлтый", variable=color, value="brown").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Синий/голубой", variable=color, value="blue").pack(
        side="left", padx=8, pady=6
    )
    ttk.Radiobutton(opts, text="Зелёный/салатовый", variable=color, value="green").pack(
        side="left", padx=8, pady=6
    )

    canv = tk.Canvas(root, bg="white")
    canv.pack(fill="both", expand=True, padx=10, pady=(6, 10))

    def run():
        try:
            n = int(ent_n.get())
        except Exception:
            messagebox.showerror("Ошибка", "N — целое.")
            return
        if n <= 0:
            messagebox.showerror("Ошибка", "N > 0.")
            return

        canv.delete("all")
        w = int(canv.winfo_width() or 720)
        h = int(canv.winfo_height() or 480)

        size = min(w, h) - 40
        x0 = (w - size) // 2
        y0 = (h - size) // 2
        cell = size // n

        if color.get() == "brown":
            c1, c2 = "#8B4513", "#DEB887"
        elif color.get() == "blue":
            c1, c2 = "#1f3b6b", "#7ec8f0"
        else:
            c1, c2 = "#1f6b3b", "#a4e2a4"

        for i in range(n):
            for j in range(n):
                # шахматка: где i+j четно — горизонтальная, иначе вертикальная
                x = x0 + j * cell
                y = y0 + i * cell
                if (i + j) % 2 == 0:
                    # горизонтальная полоска
                    canv.create_rectangle(
                        x, y + cell // 4, x + cell, y + 3 * cell // 4,
                        fill=c1, outline="black",
                    )
                else:
                    # вертикальная полоска
                    canv.create_rectangle(
                        x + cell // 4, y, x + 3 * cell // 4, y + cell,
                        fill=c2, outline="black",
                    )

    ttk.Button(root, text="Ок", command=run).pack(anchor="w", padx=10, pady=(0, 6))
    root.bind("<Return>", lambda _e: run())
    root.mainloop()


if __name__ == "__main__":
    main()
