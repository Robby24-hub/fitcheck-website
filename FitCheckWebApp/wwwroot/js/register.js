document.addEventListener("DOMContentLoaded", function () {
    const birthdayInput = document.querySelector('input[name="Birthday"]');
    const ageOutput = document.getElementById("ageOutput");

    if (birthdayInput && ageOutput) {
        birthdayInput.addEventListener("change", function () {
            const birthDate = new Date(this.value);
            const today = new Date();

            let age = today.getFullYear() - birthDate.getFullYear();
            const m = today.getMonth() - birthDate.getMonth();
            if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                age--;
            }

            ageOutput.value = age;
        });
    }
});
